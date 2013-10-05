using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteUnaryExpression
	{
		public static void Go(HaxeWriter writer, PrefixUnaryExpressionSyntax expression)
		{
			writer.Write(expression.OperatorToken.ToString()); //haxe operators are the same as C#
			Core.Write(writer, expression.Operand);
		}
		public static void Go(HaxeWriter writer, PostfixUnaryExpressionSyntax expression)
		{
			Core.Write(writer, expression.Operand);
			writer.Write(expression.OperatorToken.ToString()); //haxe operators are the same as C#
		}
	}
}
