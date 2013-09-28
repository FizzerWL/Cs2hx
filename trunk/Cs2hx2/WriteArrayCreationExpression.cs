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
			Go(writer, array, array.Initializer);
		}

		public static void Go(HaxeWriter writer, ArrayCreationExpressionSyntax array)
		{
			if (array.Type.RankSpecifiers.Count > 1)
				throw new Exception("Multi-dimensional arrays are not supported");

			if (array.Type.ElementType.ToString() == "byte")
			{
				if (array.Initializer != null)
					throw new Exception("Cannot use array initialization syntax for byte arrays");

				writer.Write("Bytes.alloc(");
				writer.Write(array.Type.RankSpecifiers[0].Sizes[0].ToString());
				writer.Write(")");
			}
			else
			{
				Go(writer, array, array.Initializer);
			}
		}

		private static void Go(HaxeWriter writer, ExpressionSyntax array, InitializerExpressionSyntax initializer)
		{
			writer.Write("[ ");

			bool first = true;
			if (initializer != null)
				foreach (var expression in initializer.Expressions)
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
