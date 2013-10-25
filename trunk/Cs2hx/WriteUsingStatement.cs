using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteUsingStatement
	{
		public static void Go(HaxeWriter writer, UsingStatementSyntax usingStatement)
		{
			if (usingStatement.DescendantNodes().OfType<ReturnStatementSyntax>().Any())
				throw new Exception("CS2HX does not support returning from within a using block. " + Utility.Descriptor(usingStatement));

			var expression = usingStatement.Expression;
			//if (expression is ExpressionStatement)
			//	expression = expression.As<ExpressionStatement>().Expression;

			//Ensure the using statement is a local variable - we can't deal with things we can't reliably repeat in the finally block
			var resource = Utility.TryGetIdentifier(expression);
			if (resource == null)
				throw new Exception("Using statements must reference a local variable. " + Utility.Descriptor(usingStatement));

			writer.WriteLine("var __disposed_" + resource + ":Bool = false;");
			writer.WriteLine("try");
			writer.WriteOpenBrace();

			if (usingStatement.Statement is BlockSyntax)
				foreach (var s in usingStatement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, s);
			else
				Core.Write(writer, usingStatement.Statement);

			writer.WriteLine("__disposed_" + resource + " = true;");
			writer.WriteLine(resource + ".Dispose();");
			writer.WriteCloseBrace();

			writer.WriteLine("catch (__catch_" + resource + ":Dynamic)");
			writer.WriteOpenBrace();
			writer.WriteLine("if (!__disposed_" + resource + ")");
			writer.WriteLine("    " + resource + ".Dispose();");
			writer.WriteLine("throw __catch_" + resource + ";");
			writer.WriteCloseBrace();
		}
	}
}
