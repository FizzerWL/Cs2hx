using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			var model = Program.GetModel(expression);

			var symbol = model.GetSymbolInfo(expression);

			var castingFrom = model.GetTypeInfo(expression.Expression).Type;
			if (castingFrom == null)
				castingFrom = model.GetTypeInfo(expression).Type;

			var castingFromStr = TypeProcessor.GenericTypeName(castingFrom);
			var castingFromHaxe = TypeProcessor.ConvertType((TypeSymbol)castingFrom);
			var destType = model.GetTypeInfo(expression.Type).Type;
			var destTypeHaxe = TypeProcessor.TryConvertType(expression.Type);

			

			var subExpression = expression.Expression;

			//Since we output parenthesis, we can eat one set of them
			if (subExpression is ParenthesizedExpressionSyntax)
				subExpression = subExpression.As<ParenthesizedExpressionSyntax>().Expression;

			if (symbol.Symbol != null && castingFromHaxe != "Int" && castingFromHaxe != "String" && castingFromHaxe != "Bool")
				WriteCastOperator(writer, expression, (MethodSymbol)symbol.Symbol, destTypeHaxe);
			else if (subExpression.Kind == SyntaxKind.NullLiteralExpression)
			{
				if (destTypeHaxe != null && destTypeHaxe.StartsWith("Nullable_"))
				{
					//Casting null to a nullable type results in creation of that nullable type. No cast necessary.
					writer.Write("new ");
					writer.Write(destTypeHaxe);
					writer.Write("()");
				}
				else
				{
					writer.Write("null"); //no cast necessary for null. haxe can infer the type better than C#
				}
			}
			else if (destTypeHaxe == null)
			{
				//Sometimes roslyn can't determine the type for some reason. Just fall back to haxe's dynamic cast
				writer.Write("cast(");
				Core.Write(writer, expression.Expression);
				writer.Write(")");
			}
			else if (castingFromHaxe == "Dynamic" 
				|| destTypeHaxe == "Dynamic"
				|| (destTypeHaxe == "Int" && castingFromHaxe == "Int" && expression.DescendantNodes().OfType<BinaryExpressionSyntax>().Where(o => o.OperatorToken.Kind == SyntaxKind.SlashToken).None()))
			{
				//Eat casts from dynamic.  haxe auto converts.
				//Eat casts from Int to Int.  Enums getting casted to int fall here, and since we use ints to represent enums anyway, it's not necessary.  However, if we contain the division operator, and since haxe division always produces floating points and C# integer division produces integers, we can't rely on the C# expression type so cast anyway.
				Core.Write(writer, expression.Expression);
			}
			else if (destTypeHaxe.Contains("<"))
			{
				//Eat casts with type parameters.  haxe doesn't allow this.
				Core.Write(writer, expression.Expression);
			}
		    else if (destTypeHaxe == "Int")
            {
                writer.Write("Std.int(");
                Core.Write(writer, subExpression);
                writer.Write(")");
            }
            else if (destTypeHaxe == "Float")
            {
                Core.Write(writer, expression.Expression);
            }
			else if (destType.TypeKind == TypeKind.TypeParameter)
			{
				//ignore casts to template types
				Core.Write(writer, expression.Expression);
			}
			else
			{
				writer.Write("cast(");
				Core.Write(writer, subExpression);
				writer.Write(", ");
				writer.Write(destTypeHaxe);
				writer.Write(")");
			}
		}

		private static void WriteCastOperator(HaxeWriter writer, CastExpressionSyntax expression, MethodSymbol symbol, string destTypeHaxe)
		{
			writer.Write(TypeProcessor.ConvertType(symbol.ContainingType));
			writer.Write(".op_Explicit_");
			writer.Write(destTypeHaxe.Replace('.', '_'));
			writer.Write("(");
			Core.Write(writer, expression.Expression);
			writer.Write(")");
		}
	}
}
