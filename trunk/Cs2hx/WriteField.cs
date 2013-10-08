using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{

    internal static class WriteField
    {
        public static void Go(HaxeWriter writer, FieldDeclarationSyntax field)
        {
            foreach (var declaration in field.Declaration.Variables)
                Go(writer, field.Modifiers, declaration.Identifier.ValueText, field.Declaration.Type, declaration.Initializer);
        }

        public static void Go(HaxeWriter writer, SyntaxTokenList modifiers, string name, TypeSyntax type, EqualsValueClauseSyntax initializerOpt = null)
        {
            writer.WriteIndent();

			var isConst = IsConst(modifiers, initializerOpt);

            if (modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.ProtectedKeyword) || modifiers.Any(SyntaxKind.InternalKeyword))
                writer.Write("public ");
            if (modifiers.Any(SyntaxKind.PrivateKeyword))
                writer.Write("private ");
            if (modifiers.Any(SyntaxKind.StaticKeyword) || modifiers.Any(SyntaxKind.ConstKeyword))
                writer.Write("static ");
			if (isConst)
				writer.Write("inline ");

            writer.Write("var ");

            writer.Write(name);
			writer.Write(TypeProcessor.ConvertTypeWithColon(type));

			if (isConst)
			{
				writer.Write(" = ");
				Core.Write(writer, initializerOpt.Value);
			}

            writer.Write(";");
            writer.WriteLine();
        }

		public static bool IsConst(SyntaxTokenList modifiers, EqualsValueClauseSyntax initializerOpt)
		{
			return modifiers.Any(SyntaxKind.ConstKeyword) || (modifiers.Any(SyntaxKind.ReadOnlyKeyword) && modifiers.Any(SyntaxKind.StaticKeyword) && initializerOpt != null);
		}
    }
}
