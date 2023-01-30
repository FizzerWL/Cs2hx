using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	static class WriteSwitchStatement
	{
		public static void Go(HaxeWriter writer, SwitchStatementSyntax switchStatement)
		{
			writer.WriteIndent();
			writer.Write("switch (");
			Core.Write(writer, switchStatement.Expression);
			writer.Write(")\r\n");
			writer.WriteOpenBrace();

			//First process all blocks except the section with the default block
			foreach (var section in switchStatement.Sections.Where(o => o.Labels.None(z => z.Keyword.Kind() == SyntaxKind.DefaultKeyword)))
			{
				writer.WriteIndent();
				writer.Write("case ");


				var firstLabel = true;
				foreach (var label in section.Labels)
				{
					if (firstLabel)
						firstLabel = false;
					else
						writer.Write(", ");

					Core.Write(writer, label.ChildNodes().Single());

				}
				writer.Write(":\r\n");
				writer.Indent++;

                //Remove any break statements at the end of the block.  If we have a single BlockSyntax node, eat it and repeat the process.
                var statements = section.Statements.ToList();
                Action clearBreaks = () =>
                {
                    if (statements.Last() is BreakStatementSyntax)
                        statements.RemoveAt(statements.Count - 1);
                };
                clearBreaks();
                if (statements.Count == 1 && statements[0] is BlockSyntax)
                {
                    statements = statements[0].As<BlockSyntax>().Statements.ToList();
                    clearBreaks();
                }

                foreach (var statement in statements)
					Core.Write(writer, statement);

				writer.Indent--;
			}

			//Now write the default section
			var defaultSection = switchStatement.Sections.SingleOrDefault(o => o.Labels.Any(z => z.Keyword.Kind() == SyntaxKind.DefaultKeyword));
			if (defaultSection != null)
			{
				if (defaultSection.Labels.Count > 1)
					throw new Exception("Cannot fall-through into or out of the default section of switch statement " + Utility.Descriptor(defaultSection));

				writer.WriteLine("default:");
				writer.Indent++;

				foreach (var statement in defaultSection.Statements)
					if (!(statement is BreakStatementSyntax))
						Core.Write(writer, statement);

				writer.Indent--;

			}


			writer.WriteCloseBrace();
		}
	}
}
