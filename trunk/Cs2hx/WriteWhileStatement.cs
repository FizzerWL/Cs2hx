using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteWhileStatement
	{
		public static void Go(HaxeWriter writer, WhileStatementSyntax whileStatement)
		{
			writer.WriteIndent();
			writer.Write("while (");
			Core.Write(writer, whileStatement.Condition);
			writer.Write(")\r\n");
			Core.Write(writer, whileStatement.Statement);
		}
	}
}
