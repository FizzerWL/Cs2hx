using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteIfStatement
	{
		public static void Go(HaxeWriter writer, IfStatementSyntax ifStatement)
		{
			writer.WriteIndent();
			writer.Write("if (");
			Core.Write(writer, ifStatement.Condition);
			writer.Write(")\r\n");


			if (ifStatement.Statement is BlockSyntax)
				Core.Write(writer, ifStatement.Statement);
			else
			{
				writer.WriteOpenBrace();
				Core.Write(writer, ifStatement.Statement);
				writer.WriteCloseBrace();
			}

			if (ifStatement.Else != null)
			{
				writer.WriteIndent();
				writer.Write("else ");

				if (ifStatement.Else.Statement is BlockSyntax)
				{
					Core.Write(writer, ifStatement.Else.Statement);
				}
				else if (ifStatement.Else.Statement is IfStatementSyntax)
				{
					WriteIfStatement.Go(writer, ifStatement.Else.Statement.As<IfStatementSyntax>());
				}
				else
				{
					writer.WriteOpenBrace();
					Core.Write(writer, ifStatement.Else.Statement);
					writer.WriteCloseBrace();
				}
			}


		}
	}
}
