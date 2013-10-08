using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteThrowStatement
	{
		public static void Go(HaxeWriter writer, ThrowStatementSyntax statement)
		{
			writer.WriteIndent();

			if (!ReturnsVoid(statement))
				writer.Write("return "); //"return" the throw statement. This works around the "return missing" haxe limitation

			writer.Write("throw ");
			Core.Write(writer, statement.Expression);
			writer.Write(";\r\n");
		}

		static bool ReturnsVoid(SyntaxNode node)
		{
			while (node != null)
			{
				var method = node as MethodDeclarationSyntax;
				if (method != null)
					return method.ReturnType.ToString() == "void";

				var prop = node as PropertyDeclarationSyntax;
				if (prop != null)
					return prop.Type.ToString() == "void";

				var lambda1 = node as ParenthesizedLambdaExpressionSyntax;
				var lambda2 = node as SimpleLambdaExpressionSyntax;
				if (lambda1 != null || lambda2 != null)
				{
					var lambda = lambda1 == null ? (ExpressionSyntax)lambda2 : (ExpressionSyntax)lambda1;
					var methodSymbol = Program.GetModel(lambda).GetTypeInfo(lambda).ConvertedType.As<NamedTypeSymbol>().DelegateInvokeMethod.As<MethodSymbol>();

					return methodSymbol.ReturnsVoid;
				}

				node = node.Parent;
			}

			throw new Exception("Node not in a body");
		}
	}
}
