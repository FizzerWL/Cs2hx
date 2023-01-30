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
	static class WriteLockStatement
	{
		public static void Go(HaxeWriter writer, LockStatementSyntax statement)
		{
			if (statement.DescendantNodes().OfType<ReturnStatementSyntax>().Any())
				throw new Exception("Cannot return from within a lock statement " + Utility.Descriptor(statement));

			writer.WriteIndent();
			writer.Write("CsLock.Lock(");
			Core.Write(writer, statement.Expression);
			writer.Write(", function()\r\n");
			writer.WriteOpenBrace();

			if (statement.Statement is BlockSyntax)
				foreach (var s in statement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, s);
			else
				Core.Write(writer, statement.Statement);

			writer.Indent--;
			writer.WriteIndent();
			writer.Write("});\r\n");

		}
	}
}
