using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteCastExpression
	{
		public static void Go(HaxeWriter writer, CastExpressionSyntax expression)
		{
			writer.Write("cast(");
			Core.Write(writer, expression.Expression);
			writer.Write(", ");
			writer.Write(TypeProcessor.ConvertType(expression.Type));
			writer.Write(")");
		}
	}
}
