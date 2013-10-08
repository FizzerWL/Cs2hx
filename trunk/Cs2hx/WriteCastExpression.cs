using System;
using System.Collections.Generic;
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
			var castingFrom = Program.GetModel(expression).GetTypeInfo(expression.Expression).Type;
			if (castingFrom == null)
				castingFrom = Program.GetModel(expression).GetTypeInfo(expression).Type;

			var castingFromStr = TypeProcessor.GenericTypeName(castingFrom);
			var castingFromHaxe = TypeProcessor.ConvertType((TypeSymbol)castingFrom);
			var destType = Program.GetModel(expression).GetTypeInfo(expression.Type).Type;
			var destTypeHaxe = TypeProcessor.ConvertType(expression.Type);

			var subExpression = expression.Expression;

			//Since we output parenthesis, we can eat one set of them
			if (subExpression is ParenthesizedExpressionSyntax)
				subExpression = subExpression.As<ParenthesizedExpressionSyntax>().Expression;


			if (destTypeHaxe == "Int" && castingFromHaxe == "Int" && expression.DescendantNodes().OfType<BinaryExpressionSyntax>().Where(o => o.OperatorToken.Kind == SyntaxKind.SlashToken).None())
			{
				//Just eat casts from Int to Int.  Enums getting casted to int fall here, and since we use ints to represent enums anyway, it's not necessary.  Unless we contain the division operator, since haxe division always produces floating points and C# integer division produces integers, so we can't rely on the C# expression type
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
			else if (castingFromHaxe == "Dynamic")
			{
				//ignore casts from dynamic as dynamic can be used as any type.  haXe throws errors when casting dynamic too, which is odd.
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
	}
}
