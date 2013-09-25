using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteReturnStatement
	{
		public static void Go(HaxeWriter writer, ReturnStatementSyntax statement)
		{
			writer.WriteIndent();
			writer.Write("return ");
			Core.Write(writer, statement.Expression);
			writer.Write(";\r\n");
		}
	}
}
