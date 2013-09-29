using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{

    internal static class WriteFields
    {
        public static void Go(HaxeWriter writer, IEnumerable<FieldDeclarationSyntax> fields)
        {
            foreach (var field in fields)
                foreach (var declaration in field.Declaration.Variables)
                    WriteField(writer, field.Modifiers, declaration.Identifier.ValueText, field.Declaration.Type, declaration.Initializer);
        }

        public static void WriteField(HaxeWriter writer, SyntaxTokenList modifiers, string name, TypeSyntax type, EqualsValueClauseSyntax initializerOpt = null)
        {
            writer.WriteIndent();

            var mods = modifiers.Select(o => o.ValueText).ToHashSet(true);

			var isConst = IsConst(modifiers, initializerOpt);

            if (mods.Contains("public") || mods.Contains("protected") || mods.Contains("internal"))
                writer.Write("public ");
            if (mods.Contains("private"))
                writer.Write("private ");
            if (mods.Contains("static") || mods.Contains("const"))
                writer.Write("static ");
			if (isConst)
				writer.Write("inline ");

            writer.Write("var ");

            writer.Write(name);
            writer.Write(":" + TypeProcessor.ConvertType(type));

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
			var mods = modifiers.Select(o => o.ValueText).ToHashSet(true);

			return mods.Contains("const") || (mods.Contains("readonly") && mods.Contains("static") && initializerOpt != null);
		}
    }
}
