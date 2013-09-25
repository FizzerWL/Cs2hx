using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteOperatorExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
		{
			Core.Write(writer, expression.Left);
			writer.Write(" ");
			writer.Write(expression.OperatorToken.ToString()); //we can do this since haxe operators work just like C# operators
			writer.Write(" ");
			Core.Write(writer, expression.Right);
		}
	}
}
