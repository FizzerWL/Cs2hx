using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteBinaryExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
		{
			Core.Write(writer, expression.Left);
			writer.Write(expression.OperatorToken.ToString()); //all of C#'s operators are identical to haxe
			Core.Write(writer, expression.Right);
		}
	}
}
