using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteArrayCreationExpression
	{
		public static void Go(HaxeWriter writer, ImplicitArrayCreationExpressionSyntax array)
		{
			Go(writer, array.Initializer);
		}

		public static void Go(HaxeWriter writer, ArrayCreationExpressionSyntax array)
		{
			Go(writer, array.Initializer);
		}

		private static void Go(HaxeWriter writer, InitializerExpressionSyntax array)
		{
			writer.Write("[ ");

			bool first = true;
			foreach (var expression in array.Expressions)
			{
				if (first)
					first = false;
				else
					writer.Write(", ");

				Core.Write(writer, expression);
			}

			writer.Write(" ]");
		}
	}
}
