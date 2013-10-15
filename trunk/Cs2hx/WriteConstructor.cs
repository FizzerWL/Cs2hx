using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

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


			if (ctorOpt != null)
			{
				var firstParameter = true;
				foreach (var parameter in ctorOpt.ParameterList.Parameters)
				{
					if (firstParameter)
						firstParameter = false;
					else
						writer.Write(", ");

					writer.Write(parameter.Identifier.ValueText);
					writer.Write(TypeProcessor.ConvertTypeWithColon(parameter.Type));

					if (parameter.Default != null)
					{
						writer.Write(" = ");
						Core.Write(writer, parameter.Default.Value);
					}
				}
			}

            writer.Write(")\r\n");
            writer.WriteOpenBrace();

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

			foreach (var field in TypeState.Instance.AllMembers
						.OfType<BaseFieldDeclarationSyntax>()
						.Where(o => !o.Modifiers.Any(SyntaxKind.StaticKeyword))
						.SelectMany(o => o.Declaration.Variables)
						.Where(o =>
							(o.Initializer != null && !WriteField.IsConst(o.Parent.Parent.As<BaseFieldDeclarationSyntax>().Modifiers, o.Initializer))
							||
							(o.Initializer == null && TypeProcessor.ValueToReference(o.Parent.As<VariableDeclarationSyntax>().Type))
							||
							o.Parent.Parent is EventFieldDeclarationSyntax))
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
                writer.Write(" = ");

				if (field.Parent.Parent is EventFieldDeclarationSyntax)
				{
					writer.Write("new CsEvent<");
					writer.Write(TypeProcessor.ConvertType(field.Parent.As<VariableDeclarationSyntax>().Type));
					writer.Write(">()");
				}
				else if (field.Initializer == null)
				{
					//The only way to get here with a null initializer is for a TypeProcess.ValueToReference field.
					writer.Write("new ");
					writer.Write(TypeProcessor.ConvertType(field.Parent.As<VariableDeclarationSyntax>().Type));
					writer.Write("()");
				}
				else
				{
					Core.Write(writer, field.Initializer.Value);
				}

                writer.Write(";\r\n");
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
			var staticFieldsNeedingInitialization = TypeState.Instance.AllMembers
				.OfType<BaseFieldDeclarationSyntax>()
				.Where(o => o.Modifiers.Any(SyntaxKind.StaticKeyword))
				.SelectMany(o => o.Declaration.Variables)
				.Where(o =>
					(o.Initializer != null && !WriteField.IsConst(o.Parent.Parent.As<BaseFieldDeclarationSyntax>().Modifiers, o.Initializer))
					||
					(o.Initializer == null && TypeProcessor.ValueToReference(o.Parent.As<VariableDeclarationSyntax>().Type))
					||
					o.Parent.Parent is EventFieldDeclarationSyntax)
				.ToList();

            if (staticConstructorOpt == null && staticFieldsNeedingInitialization.Count == 0)
                return; //No static constructor needed

            writer.WriteLine("public static function cctor():Void");
            writer.WriteOpenBrace();

			foreach (var field in staticFieldsNeedingInitialization)
            {
                writer.WriteIndent();
                writer.Write(field.Identifier.ValueText);
				writer.Write(" = ");

				if (field.Parent.Parent is EventFieldDeclarationSyntax)
				{
					writer.Write("new CsEvent<");
					writer.Write(TypeProcessor.ConvertType(field.Parent.As<VariableDeclarationSyntax>().Type));
					writer.Write(">()");
				}
				else if (field.Initializer == null)
				{
					//The only way to get here without an initializer is if it's a TypeProcessor.ValueToReference.
					writer.Write("new ");
					writer.Write(TypeProcessor.ConvertType(field.Parent.As<VariableDeclarationSyntax>().Type));
					writer.Write("()");
				}
				else
				{
					Core.Write(writer, field.Initializer.As<EqualsValueClauseSyntax>().Value);
				}
				writer.Write(";\r\n");
            }

			if (staticConstructorOpt != null && staticConstructorOpt.Body != null)
				foreach (var statement in staticConstructorOpt.Body.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);

            writer.WriteCloseBrace();

            Program.StaticConstructors.Add(TypeState.Instance.TypeName);
        }

    }
}
