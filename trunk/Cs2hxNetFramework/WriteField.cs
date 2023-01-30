using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{

    internal static class WriteField
    {
        public static void Go(HaxeWriter writer, FieldDeclarationSyntax field)
        {
            foreach (var declaration in field.Declaration.Variables)
                Go(writer, field.Modifiers, declaration.Identifier.ValueText, field.Declaration.Type, declaration.Initializer);
        }

		public static void WriteFieldModifiers(HaxeWriter writer, SyntaxTokenList modifiers)
		{
			if (modifiers.Any(SyntaxKind.PublicKeyword) || modifiers.Any(SyntaxKind.ProtectedKeyword) || modifiers.Any(SyntaxKind.InternalKeyword))
				writer.Write("public ");
			if (modifiers.Any(SyntaxKind.PrivateKeyword))
				writer.Write("private ");
			if (modifiers.Any(SyntaxKind.StaticKeyword) || modifiers.Any(SyntaxKind.ConstKeyword))
				writer.Write("static ");
		}

        public static void Go(HaxeWriter writer, SyntaxTokenList modifiers, string name, TypeSyntax type, EqualsValueClauseSyntax initializerOpt = null)
        {
            writer.WriteIndent();

			var isConst = IsConst(modifiers, initializerOpt, type);

			WriteFieldModifiers(writer, modifiers);
			if (isConst)
				writer.Write("inline ");

            writer.Write("var ");

            writer.Write(name);
			writer.Write(TypeProcessor.ConvertTypeWithColon(type));

            if (initializerOpt != null)
            {
                writer.Write(" = ");
                Core.Write(writer, initializerOpt.Value);
            }
            else if (GenerateInitializerForFieldWithoutInitializer(type))
            {
                writer.Write(" = ");
                if (TypeProcessor.ValueToReference(type))
                {
                    writer.Write("new ");
                    writer.Write(TypeProcessor.ConvertType(type));
                    writer.Write("()");
                }
                else
                {
                    writer.Write(TypeProcessor.DefaultValue(TypeProcessor.ConvertType(type)));
                }
            }


            writer.Write(";");
            writer.WriteLine();
        }

		public static bool IsConst(SyntaxTokenList modifiers, EqualsValueClauseSyntax initializerOpt, TypeSyntax type)
		{
			var t = TypeProcessor.ConvertType(type);

			return (modifiers.Any(SyntaxKind.ConstKeyword)
				|| (modifiers.Any(SyntaxKind.ReadOnlyKeyword) && modifiers.Any(SyntaxKind.StaticKeyword) && initializerOpt != null))
				&& (t == "Int" || t == "String" || t == "Bool" || t == "Float");
		}


        private static bool GenerateInitializerForFieldWithoutInitializer(TypeSyntax parentType)
        {
            //Determine if we need to write an initializer for this field which does not have an initializer.  
            if (TypeProcessor.ValueToReference(parentType))
                return true;
            else
                return TypeProcessor.DefaultValue(TypeProcessor.ConvertType(parentType)) != "null";
        }

    }
}
