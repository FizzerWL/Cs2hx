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
	static class WriteParenthesizedExpression
	{
		public static void Go(HaxeWriter writer, ParenthesizedExpressionSyntax expression)
		{
			writer.Write("(");
			Core.Write(writer, expression.Expression);
			writer.Write(")");
		}
	}
}
