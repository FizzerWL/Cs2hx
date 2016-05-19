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
	static class WriteReturnStatement
	{
		public static void Go(HaxeWriter writer, ReturnStatementSyntax statement)
		{
			writer.WriteIndent();
			writer.Write("return");

			if (statement.Expression != null)
			{
				writer.Write(" ");
				Core.Write(writer, statement.Expression);
			}


			writer.Write(";\r\n");
		}
	}
}
