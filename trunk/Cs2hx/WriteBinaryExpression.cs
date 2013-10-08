using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteBinaryExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
		{
			if (expression.OperatorToken.Kind != SyntaxKind.EqualsToken && expression.OperatorToken.Kind != SyntaxKind.PlusToken)
			{
				var model = Program.GetModel(expression);
				Func<ExpressionSyntax, bool> isNullable = e =>
					{
						var t = model.GetTypeInfo(e).Type;
						return t != null && t.Name == "Nullable";
					};

				if (isNullable(expression.Left) || isNullable(expression.Right))
					throw new Exception("When using nullable types, you must use the .Value and .HasValue properties instead of the object directly " + Utility.Descriptor(expression));
			}


			if (expression.OperatorToken.Kind == SyntaxKind.AsKeyword)
				throw new NotImplementedException("\"as\" keyword not supported yet");
			else if (expression.OperatorToken.Kind == SyntaxKind.IsKeyword)
			{
				writer.Write("Std.is(");
				Core.Write(writer, expression.Left);
				writer.Write(", ");
				writer.Write(TypeProcessor.RemoveGenericArguments(TypeProcessor.ConvertType(expression.Right)));
				writer.Write(")");
			}
			else if (expression.OperatorToken.Kind == SyntaxKind.EqualsToken)
			{
				//Write assignment
				Core.Write(writer, expression.Left);
				writer.Write(" = ");
				WriteAssignment(writer, expression.Right);
			}
			else
			{
				Core.Write(writer, expression.Left);
				writer.Write(" ");
				writer.Write(expression.OperatorToken.ToString()); //we can do this since haxe operators work just like C# operators
				writer.Write(" ");
				Core.Write(writer, expression.Right);
			}

			
		}

		public static void WriteAssignment(HaxeWriter writer, ExpressionSyntax expression)
		{
			var rightType = Program.GetModel(expression).GetTypeInfo(expression);

			if (rightType.ConvertedType.Name == "Nullable" && (rightType.Type == null || rightType.Type.Name != "Nullable"))
			{
				//When assigning into a nullable, we must construct the nullable type.
				writer.Write("new ");
				writer.Write(TypeProcessor.ConvertType(rightType.ConvertedType));
				writer.Write("(");

				if (rightType.Type != null)
					Core.Write(writer, expression);

				writer.Write(")");
			}
			else
				Core.Write(writer, expression);
		}

	}
}
