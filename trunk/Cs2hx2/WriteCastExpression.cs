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
			var castingFrom = TypeState.Instance.GetModel(expression).GetTypeInfo(expression.Expression).ConvertedType;
			var castingFromStr = TypeProcessor.GenericTypeName(castingFrom);
			var castingFromHaxe = TypeProcessor.ConvertType((TypeSymbol)castingFrom);
			var destType = TypeState.Instance.GetModel(expression).GetTypeInfo(expression.Type).Type;
			var destTypeHaxe = TypeProcessor.ConvertType(expression.Type);


			if (destTypeHaxe == "Int" && castingFromHaxe == "Int")
			{
				//Just eat casts from Int to Int.  Enums getting casted to int fall here, and since we use ints to represent enums anyway, it's not necessary
				Core.Write(writer, expression.Expression);
			}
		    else if (destTypeHaxe == "Int")
            {
                writer.Write("Std.int(");
                Core.Write(writer, expression.Expression);
                writer.Write(")");
            }
            else if (destTypeHaxe == "Float")
            {
                Core.Write(writer, expression.Expression);
            }
			else if (destType.TypeKind == Roslyn.Compilers.Common.CommonTypeKind.TypeParameter)
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
				Core.Write(writer, expression.Expression);
				writer.Write(", ");
				writer.Write(destTypeHaxe);
				writer.Write(")");
			}
		}
	}
}
