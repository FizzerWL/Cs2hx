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
				writer.Write("haxe.io.Bytes.alloc(");
				Core.Write(writer, array.Type.RankSpecifiers[0].Sizes[0]);
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
