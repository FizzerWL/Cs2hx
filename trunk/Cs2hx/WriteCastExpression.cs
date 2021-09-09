using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
			var castingFromHaxe = TypeProcessor.ConvertType((ITypeSymbol)castingFrom);
			var destType = model.GetTypeInfo(expression.Type).Type;
			var destTypeHaxe = TypeProcessor.TryConvertType(expression.Type);

			

			var subExpression = expression.Expression;

			//Since we output parenthesis, we can eat one set of them
			if (subExpression is ParenthesizedExpressionSyntax)
				subExpression = subExpression.As<ParenthesizedExpressionSyntax>().Expression;


            if (symbol.Symbol != null && castingFromHaxe != "Int" && castingFromHaxe != "String" && castingFromHaxe != "Bool" && castingFromHaxe != "Float")
			{
                //when the symbol is non-null, this indicates we're calling a cast operator function
                WriteCastOperator(writer, expression, (IMethodSymbol)symbol.Symbol, destTypeHaxe);
			}
			else if (destTypeHaxe.StartsWith("Nullable"))
			{
				//Casting to a nullable type results in creation of that nullable type. No cast necessary.
				writer.Write("new ");
				writer.Write(destTypeHaxe);
				writer.Write("(");

				if (subExpression.Kind() != SyntaxKind.NullLiteralExpression)
					Core.Write(writer, subExpression);

				writer.Write(")");
			}
			else if (subExpression.Kind() == SyntaxKind.NullLiteralExpression)
			{
				//no cast necessary for null. haxe can infer the type better than C#
				writer.Write("null"); 
			}
			else if (destTypeHaxe == null)
			{
				//Sometimes roslyn can't determine the type for some reason. Just fall back to haxe's dynamic cast
				writer.Write("cast(");
				Core.Write(writer, subExpression);
				writer.Write(")");
			}
            else if (castingFromHaxe == "Dynamic")
			{
                //casts to and from dynamic will be automatic, however we have to manually specify the type.  Otherwise, we saw cases on the js target where haxe would not translate properties from "X" to "get_X()".
                //The only way I've found to specify the type of an expression is to in-line an anonymous function.  Haxe optimizes this away so there's no runtime hit.
                writer.Write("(function():" + destTypeHaxe + " return ");
                Core.Write(writer, expression.Expression);
                writer.Write(")()");
			}
			else if (destTypeHaxe == "Int" && castingFromHaxe == "Int" && expression.DescendantNodes().OfType<BinaryExpressionSyntax>().Where(o => o.OperatorToken.Kind() == SyntaxKind.SlashToken).None())
			{
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
				//Casting from int to float is handled by Std.int in haxe
				writer.Write("Std.int(");
				Core.Write(writer, subExpression);
				writer.Write(")");
			}
			else if (destTypeHaxe == "Float" && castingFromHaxe == "Int")
			{
				//Eat casts from Int to Float.  C# does this so it can do floating division, but in haxe all division is floating so there's no reason to do it.
				Core.Write(writer, expression.Expression);
			}
			else if (destTypeHaxe == castingFromHaxe)
			{
				//Eat casts between identical types
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

		private static void WriteCastOperator(HaxeWriter writer, CastExpressionSyntax expression, IMethodSymbol symbol, string destTypeHaxe)
		{
			writer.Write(TypeProcessor.ConvertType(symbol.ContainingType));
			writer.Write(".op_Explicit_");
			writer.Write(destTypeHaxe.TrySubstringBeforeFirst('<').Replace('.', '_'));
			writer.Write("(");
			Core.Write(writer, expression.Expression);
			writer.Write(")");
		}

	}
}
