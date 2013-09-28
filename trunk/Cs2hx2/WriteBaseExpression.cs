using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteBaseExpression
	{
		public static void Go(HaxeWriter writer, BaseExpressionSyntax expression)
		{
			writer.Write("super");
		}
	}
}
