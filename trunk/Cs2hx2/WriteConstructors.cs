using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class WriteConstructors
    {
        public static void Go(HaxeWriter writer, IEnumerable<ConstructorDeclarationSyntax> constructors, bool derivesFromObject, IEnumerable<VariableDeclaratorSyntax> instanceFieldsNeedingInitialization, IEnumerable<VariableDeclaratorSyntax> staticFieldsNeedingInitialization)
        {
            var staticConstructor = constructors.SingleOrDefault(o => o.Modifiers.Any(SyntaxKind.StaticKeyword));

            WriteStaticConstructor(writer, staticConstructor, staticFieldsNeedingInitialization);

            var normalctors = constructors.Except(staticConstructor);

            if (normalctors.Count() > 1)
                throw new Exception("Overloaded constructors not allowed: " + Utility.Descriptor(normalctors.First()));

            ConstructorDeclarationSyntax ctor;

            if (normalctors.Count() == 0)
                ctor = null;
            else
                ctor = normalctors.Single();

            writer.WriteIndent();

            writer.Write("public function new(");


            if (ctor != null)
            {
                var firstParameter = true;
                foreach (var parameter in ctor.ParameterList.Parameters)
                {
                    if (firstParameter)
                        firstParameter = false;
                    else
                        writer.Write(", ");

                    writer.Write(parameter.Identifier.ValueText);
					writer.Write(":" + TypeProcessor.ConvertType(parameter.Type));

					if (parameter.Default != null)
					{
						writer.Write(" = ");
						Core.Write(writer, parameter.Default.Value);
					}
                }
            }

            writer.Write(")\r\n");
            writer.WriteOpenBrace();

            if (!derivesFromObject)
            {
				if (ctor == null || ctor.Initializer == null)
					writer.WriteLine("super();");
				else
				{
					if (ctor.Initializer.ThisOrBaseKeyword.ToString() != "base")
						throw new Exception("Constructor overloading not supported " + Utility.Descriptor(ctor));

					writer.WriteIndent();
					writer.Write("super(");

					bool first = true;
					foreach (var init in ctor.Initializer.ArgumentList.Arguments)
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

            foreach (var field in instanceFieldsNeedingInitialization)
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
                writer.Write(" = ");
                Core.Write(writer, field.Initializer.Value);
                writer.Write(";\r\n");
            }

            if (ctor != null && ctor.Body != null)
				foreach(var statement in ctor.Body.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);

            writer.WriteCloseBrace();
        }



        private static void WriteStaticConstructor(HaxeWriter writer, ConstructorDeclarationSyntax staticConstructor, IEnumerable<VariableDeclaratorSyntax> staticInitializationNeeded)
        {
            if (staticConstructor == null && staticInitializationNeeded.Count() == 0)
                return; //No static constructor needed

            writer.WriteLine("public static function cctor():Void");
            writer.WriteOpenBrace();

            foreach (var field in staticInitializationNeeded)
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
                writer.Write(" = ");
				Core.Write(writer, field.Initializer);
                writer.Write(";\r\n");
            }

            if (staticConstructor != null && staticConstructor.Body != null)
				Core.Write(writer, staticConstructor.Body);

            writer.WriteCloseBrace();

            Program.StaticConstructors.Add(TypeState.Instance.TypeName);
        }

    }
}
