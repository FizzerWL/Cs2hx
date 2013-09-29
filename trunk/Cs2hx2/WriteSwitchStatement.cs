using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

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
			foreach (var section in switchStatement.Sections)
			{
				if (section.Labels.Count > 1)
					throw new Exception("haXe does not support falling through from one case statement to another " + Utility.Descriptor(section));

				foreach (var label in section.Labels)
				{
					writer.WriteIndent();

					if (label.CaseOrDefaultKeyword.Kind == SyntaxKind.DefaultKeyword)
						writer.Write("default");
					else
					{
						writer.Write("case ");
						Core.Write(writer, label.Value);
					}

					writer.Write(":\r\n");
				}
				writer.Indent++;

				foreach (var statement in section.Statements)
					if (!(statement is BreakStatementSyntax))
						Core.Write(writer, statement);

				writer.Indent--;
			}
			writer.WriteCloseBrace();
		}
	}
}
