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
	static class WriteDoStatement
	{
		public static void Go(HaxeWriter writer, DoStatementSyntax statement)
		{
			writer.WriteLine("do");
			writer.WriteOpenBrace();

			if (statement.Statement is BlockSyntax)
				foreach (var s in statement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, s);
			else
				Core.Write(writer, statement.Statement);

			writer.WriteCloseBrace();
			writer.WriteIndent();
			writer.Write("while (");
			Core.Write(writer, statement.Condition);
			writer.Write(");\r\n");
		}
	}
}
