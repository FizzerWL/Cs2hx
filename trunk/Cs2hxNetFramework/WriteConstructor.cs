using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cs2hx.Translations;

namespace Cs2hx
{
    static class WriteConstructor
    {


        public static void Go(HaxeWriter writer, ConstructorDeclarationSyntax constructor)
        {
			if (constructor.Modifiers.Any(SyntaxKind.StaticKeyword))
				WriteStaticConstructor(writer, constructor);
			else
				WriteInstanceConstructor(writer, constructor);
		}

		public static void WriteInstanceConstructor(HaxeWriter writer, ConstructorDeclarationSyntax ctorOpt)
		{

            writer.WriteIndent();

            writer.Write("public function new(");

            Dictionary<string, ExpressionSyntax> deferredDefaults = null;

            if (ctorOpt != null)
			{
                var methodSymbol = Program.GetModel(ctorOpt).GetDeclaredSymbol(ctorOpt);
                WriteMethod.WriteParameters(writer, ctorOpt, methodSymbol, out deferredDefaults);
			}

            writer.Write(")\r\n");
            writer.WriteOpenBrace();

            if (deferredDefaults != null)
            {
                foreach (var defer in deferredDefaults)
                {
                    writer.WriteLine("if (" + defer.Key + " == null)");
                    writer.Indent++;
                    writer.WriteIndent();
                    writer.Write(defer.Key);
                    writer.Write(" = ");
                    Core.Write(writer, defer.Value);
                    writer.Write(";\r\n");
                    writer.Indent--;
                }
            }

            if (!TypeState.Instance.DerivesFromObject)
            {
				if (ctorOpt == null || ctorOpt.Initializer == null)
					writer.WriteLine("super();");
				else
				{
					if (ctorOpt.Initializer.ThisOrBaseKeyword.ToString() != "base")
						throw new Exception("Constructor overloading not supported " + Utility.Descriptor(ctorOpt));

					writer.WriteIndent();
					writer.Write("super(");

					bool first = true;
					foreach (var init in ctorOpt.Initializer.ArgumentList.Arguments)
					{
						if (first)
							first = false;
						else
							writer.Write(", ");

						Core.Write(writer, init.Expression);
					}

					writer.Write(");\r\n");
				}

            }
            

			if (ctorOpt != null && ctorOpt.Body != null)
			{
				foreach (var statement in ctorOpt.Body.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);

				TriviaProcessor.ProcessTrivias(writer, ctorOpt.Body.DescendantTrivia());
			}

            writer.WriteCloseBrace();
        }


        public static void WriteStaticConstructor(HaxeWriter writer, ConstructorDeclarationSyntax staticConstructorOpt)
        {
            if (staticConstructorOpt == null)
                return; //No static constructor needed

            writer.WriteLine("public static function cctor():Void");
            writer.WriteOpenBrace();
            

			if (staticConstructorOpt != null && staticConstructorOpt.Body != null)
				foreach (var statement in staticConstructorOpt.Body.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);

            writer.WriteCloseBrace();

            StaticConstructors.Add(TypeState.Instance.TypeName);
        }

		static HashSet<string> StaticConstructors = new HashSet<string>();
		static HashSet<string> AllTypes = new HashSet<string>();


		public static void WriteConstructorsHelper(IEnumerable<INamedTypeSymbol> allTypes, string nameArg)
		{
			foreach(var t in allTypes.Select(o => o.ContainingNamespace.FullNameWithDot().ToLower() + WriteType.TypeName(o)))
				AllTypes.Add(t);

            var name = string.IsNullOrWhiteSpace(nameArg) ? "Constructors" : nameArg;


            using (var writer = new HaxeWriter("", name))
			{
				writer.WriteLine(@"/*
This file serves two purposes:  
    1)  It imports every type that CS2HX generated.  haXe will ignore 
        any types that aren't used by haXe code, so this ensures haXe 
        compiles all of your code.

    2)  It lists all the static constructors.  haXe doesn't have the 
        concept of static constructors, so CS2HX generated cctor()
        methods.  You must call these manually.  If you call
        Constructors.init(), all static constructors will be called 
        at once.
*/
package ;");

				foreach (var type in AllTypes.OrderBy(o => o))
					writer.WriteLine("import " + type + ";");
				writer.WriteLine("import system.TimeSpan;");

				writer.WriteLine("class " + name);
				writer.WriteOpenBrace();

				writer.WriteLine("public static function init()");
				writer.WriteOpenBrace();
				writer.WriteLine("TimeSpan.cctor();");
				foreach (var cctor in StaticConstructors.OrderBy(o => o))
					writer.WriteLine(cctor + ".cctor();");
				writer.WriteCloseBrace();
				writer.WriteCloseBrace();
			}
		}

    }
}
