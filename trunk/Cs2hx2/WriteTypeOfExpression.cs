using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteTypeOfExpression
	{
		public static void Go(HaxeWriter writer, TypeOfExpressionSyntax expression)
		{
			writer.Write("typeof(");
			writer.Write(TypeProcessor.ConvertType(expression.Type));
			writer.Write(")");
		}
	}
}
