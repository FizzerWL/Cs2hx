using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteMemberAccessExpression
	{
		public static void Go(HaxeWriter writer, MemberAccessExpressionSyntax expression)
		{
			Core.Write(writer, expression.Expression);
			writer.Write(".");
			writer.Write(expression.Name.Identifier.ValueText);

			if (expression.Name is GenericNameSyntax)
			{
				var gen = expression.Name.As<GenericNameSyntax>();

				writer.Write("<");

				bool first = true;
				foreach (var g in gen.TypeArgumentList.Arguments)
				{
					if (first)
						first = false;
					else
						writer.Write(", ");

					writer.Write(":" + TypeProcessor.ConvertType(g));
				}

				writer.Write(">");
			}
		}
	}
}
