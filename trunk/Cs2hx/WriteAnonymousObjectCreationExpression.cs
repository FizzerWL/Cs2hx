using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteAnonymousObjectCreationExpression
	{
		public static void Go(HaxeWriter writer, AnonymousObjectCreationExpressionSyntax expression)
		{
			writer.Write(" { ");

			bool first = true;
			foreach (var field in expression.Initializers)
			{
				if (first)
					first = false;
				else
					writer.Write(", ");

				Core.Write(writer, field.NameEquals.Name);
				writer.Write(": ");
				Core.Write(writer, field.Expression);
			}

			writer.Write(" }");
		}
	}
}
