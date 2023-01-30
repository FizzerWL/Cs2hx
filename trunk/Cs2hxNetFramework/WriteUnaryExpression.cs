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
	static class WriteUnaryExpression
	{
		public static void Go(HaxeWriter writer, PrefixUnaryExpressionSyntax expression)
		{
			writer.Write(expression.OperatorToken.ToString()); //haxe operators are the same as C#
			Core.Write(writer, expression.Operand);
		}
		public static void Go(HaxeWriter writer, PostfixUnaryExpressionSyntax expression)
		{
			//Check for ++ and -- after a element access, such as a dictionary<int,int> being called as dict[4]++
			if (expression.Operand is ElementAccessExpressionSyntax)
			{
				var elementAccess = (ElementAccessExpressionSyntax)expression.Operand;

				var typeHaxe = TypeProcessor.ConvertType(Program.GetModel(expression).GetTypeInfo(elementAccess.Expression).ConvertedType);

				if (!typeHaxe.StartsWith("Array<")) //arrays are the only thing haxe allows assignments into via indexing
				{
					WriteBinaryExpression.WriteIndexingExpression(writer, expression.OperatorToken, null, elementAccess);
					return;
				}

			}

			Core.Write(writer, expression.Operand);
			writer.Write(expression.OperatorToken.ToString()); //haxe operators are the same as C#
		}
	}
}
