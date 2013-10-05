using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteThrowStatement
	{
		public static void Go(HaxeWriter writer, ThrowStatementSyntax statement)
		{
			writer.WriteIndent();
			writer.Write("throw ");
			Core.Write(writer, statement.Expression);
			writer.Write(";\r\n");
		}
	}
}
