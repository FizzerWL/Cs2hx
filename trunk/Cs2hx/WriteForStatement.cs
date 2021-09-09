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
	static class WriteForStatement
	{
		public static void Go(HaxeWriter writer, ForStatementSyntax forStatement)
		{
			if (forStatement.DescendantNodes().OfType<ContinueStatementSyntax>().Any())
				throw new Exception("Cannot use \"continue\" in a \"for\" loop.  Consider changing to a while loop instead. " + Utility.Descriptor(forStatement));

			writer.WriteLine("{ //for");
			writer.Indent++;

			if (forStatement.Declaration != null)
				foreach (var variable in forStatement.Declaration.Variables)
				{
					writer.WriteIndent();
					writer.Write("var ");
					writer.Write(variable.Identifier.ValueText);
					writer.Write(TypeProcessor.ConvertTypeWithColon(forStatement.Declaration.Type));

					if (variable.Initializer != null)
					{
						writer.Write(" = ");
						Core.Write(writer, variable.Initializer.Value);
					}

					writer.Write(";\r\n");
				}

			foreach (var init in forStatement.Initializers)
			{
				writer.WriteIndent();
				Core.Write(writer, init);
				writer.Write(";\r\n");
			}

			writer.WriteIndent();
			writer.Write("while (");

			if (forStatement.Condition == null)
				writer.Write("true");
			else
				Core.Write(writer, forStatement.Condition);

			writer.Write(")\r\n");
			writer.WriteOpenBrace();

			if (forStatement.Statement is BlockSyntax)
			{
				foreach (var statement in forStatement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);
			}
			else
				Core.Write(writer, forStatement.Statement);

			foreach (var iterator in forStatement.Incrementors)
			{
				writer.WriteIndent();
				Core.Write(writer, iterator);
				writer.Write(";\r\n");
			}

			writer.WriteCloseBrace();
			writer.Indent--;
			writer.WriteLine("} //end for");
		}
	}
}
