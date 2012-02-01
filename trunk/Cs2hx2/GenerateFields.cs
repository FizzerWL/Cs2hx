using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{

    internal static class GenerateFields
    {
        public static void Go(HaxeWriter writer, IEnumerable<FieldDeclarationSyntax> fields)
        {
            foreach (var field in fields)
                foreach (var declaration in field.Declaration.Variables)
                    WriteField(writer, field.Modifiers, declaration.Identifier.ValueText, field.Declaration.Type);
        }

        public static void WriteField(HaxeWriter writer, SyntaxTokenList modifiers, string name, TypeSyntax type)
        {
            writer.WriteIndent();

            var mods = modifiers.Select(o => o.ValueText).ToHashSet(true);

            if (mods.Contains("public") || mods.Contains("protected") || mods.Contains("internal"))
                writer.Write("public ");
            if (mods.Contains("private"))
                writer.Write("private ");
            if (mods.Contains("static") || mods.Contains("const"))
                writer.Write("static ");

            writer.Write("var ");

            writer.Write(name);
            writer.Write(TypeProcessor.TryConvertType(type));
            writer.Write(";");
            writer.WriteLine();
        }
    }
}
