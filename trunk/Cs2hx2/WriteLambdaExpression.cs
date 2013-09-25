using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteLambdaExpression
	{
		public static void Go(HaxeWriter writer, ParenthesizedLambdaExpressionSyntax expression)
		{
			Go(writer, expression.ParameterList.Parameters, expression.Body);
		}

		public static void Go(HaxeWriter writer, SimpleLambdaExpressionSyntax expression)
		{
			Go(writer, new[] { expression.Parameter }, expression.Body);
		}

		private static void Go(HaxeWriter writer, IEnumerable<ParameterSyntax> parameters, SyntaxNode body)
		{
			var type = TypeState.Instance.GetModel(body).GetTypeInfo(body.Parent);
			var methodSymbol = type.ConvertedType.As<NamedTypeSymbol>().DelegateInvokeMethod.As<MethodSymbol>();

			writer.Write("function (");

			for(int pi = 0; pi < parameters.Count();pi++)
			{
				var parameter = parameters.ElementAt(pi);
				if (pi > 0)
					writer.Write(", ");

				writer.Write(parameter.Identifier.ValueText);
				if (parameter.Type != null)
					writer.Write(":" + TypeProcessor.ConvertType(parameter.Type));
				else
					writer.Write(":" + TypeProcessor.ConvertType(methodSymbol.Parameters[pi].Type));
			}

			writer.Write(")");
			
			writer.Write(" { ");

			if (methodSymbol.ReturnsVoid == false)
				writer.WriteLine("return ");

			Core.Write(writer, body);

			

			writer.Write("; } ");
		}
	}
}
