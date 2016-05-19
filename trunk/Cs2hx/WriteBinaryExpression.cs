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
	static class WriteBinaryExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
        {
            Go(writer, expression.Left, expression.OperatorToken, expression.Right);
        }

        public static void Go(HaxeWriter writer, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
			//Check for index assignments, for example dictionary[4] = 3;
			if (left is ElementAccessExpressionSyntax && operatorToken.Kind() == SyntaxKind.EqualsToken)
			{
				var elementAccess = (ElementAccessExpressionSyntax)left;

				var typeHaxe = TypeProcessor.ConvertType(Program.GetModel(left).GetTypeInfo(elementAccess.Expression).ConvertedType);

				if (!typeHaxe.StartsWith("Array<")) //arrays are the only thing haxe allows assignments into via indexing
				{
					WriteIndexingExpression(writer, operatorToken, right, elementAccess);
					return;
				}
			}


			//Check for improper nullable access, unless it's assignment or string concat, which work fine.  Note that if they're trying to add nullable number types, it won't catch it since we can't tell if it's string concatentation or addition.  But haxe will fail to build, so it should still be caught easily enough.
			if (operatorToken.Kind() != SyntaxKind.EqualsToken && operatorToken.Kind() != SyntaxKind.PlusToken)
			{
				var model = Program.GetModel(left);
				Func<ExpressionSyntax, bool> isNullable = e =>
					{
						var t = model.GetTypeInfo(e).Type;
						return t != null && t.Name == "Nullable";
					};

				if (isNullable(left) || isNullable(right))
					throw new Exception("When using nullable types, you must use the .Value and .HasValue properties instead of the object directly " + Utility.Descriptor(left.Parent));
			}

			if (operatorToken.Kind() == SyntaxKind.PlusEqualsToken || operatorToken.Kind() == SyntaxKind.MinusEqualsToken)
			{
				//Check for event subscription/removal
				var leftSymbol = Program.GetModel(left).GetSymbolInfo(left);
				if (leftSymbol.Symbol is IEventSymbol)
				{
					Core.Write(writer, left);
					if (operatorToken.Kind() == SyntaxKind.PlusEqualsToken)
						writer.Write(".Add(");
					else
						writer.Write(".Remove(");
					Core.Write(writer, right);
					writer.Write(")");
					return;
				}
			}


			if (operatorToken.Kind() == SyntaxKind.AsKeyword)
			{
				var leftStr = Utility.TryGetIdentifier(left);

				if (leftStr == null)
					throw new Exception("The \"as\" keyword can only be used on simple names.  Declare it as a local variable. " + Utility.Descriptor(left.Parent));

				var typeHaxe = TypeProcessor.ConvertType(right);

				writer.Write("(Std.is(" + leftStr + ", " + typeHaxe + ") ? cast(" + leftStr + ", " + typeHaxe + ") : null)");

			}
			else if (operatorToken.Kind() == SyntaxKind.IsKeyword)
			{
				writer.Write("Std.is(");
				Core.Write(writer, left);
				writer.Write(", ");
				writer.Write(TypeProcessor.RemoveGenericArguments(TypeProcessor.ConvertType(right)));
				writer.Write(")");
			}
			else if (operatorToken.Kind() == SyntaxKind.QuestionQuestionToken)
			{
				writer.Write("Cs2Hx.Coalesce(");
				Core.Write(writer, left);
				writer.Write(", ");
				Core.Write(writer, right);
				writer.Write(")");
			}
			else
			{
				Action<ExpressionSyntax, ExpressionSyntax> write = (e, otherSide) =>
					{
                        var type = Program.GetModel(left).GetTypeInfo(e);
                        var otherType = Program.GetModel(left).GetTypeInfo(otherSide);
						//Check for enums being converted to strings by string concatenation
						if (operatorToken.Kind() == SyntaxKind.PlusToken && type.Type.TypeKind == TypeKind.Enum)
						{
							writer.Write(type.Type.ContainingNamespace.FullNameWithDot().ToLower());
							writer.Write(WriteType.TypeName(type.Type.As<INamedTypeSymbol>()));
							writer.Write(".ToString(");
							Core.Write(writer, e);
							writer.Write(")");
						}
                        else if (operatorToken.Kind() == SyntaxKind.PlusToken && type.Type.SpecialType == SpecialType.System_Char && otherType.Type.SpecialType != SpecialType.System_Char && otherType.Type.SpecialType != SpecialType.System_Int32)
                        {
                            //Chars are integers in haxe, so when string-concatening them we must convert them to strings
                            writer.Write("Cs2Hx.CharToString(");
                            Core.Write(writer, e);
                            writer.Write(")");
                        }
                        else
							Core.Write(writer, e);
					};

				write(left, right);
				writer.Write(" ");
				writer.Write(operatorToken.ToString()); //we can do this since haxe operators work just like C# operators
				writer.Write(" ");
				write(right, left);
			}

			
		}

		public static void WriteIndexingExpression(HaxeWriter writer, SyntaxToken operatorToken, ExpressionSyntax subExpressionOpt, ElementAccessExpressionSyntax elementAccess)
		{
			Core.Write(writer, elementAccess.Expression);
			writer.Write(".");

			Action writeArgs = () =>
				{
					foreach (var arg in elementAccess.ArgumentList.Arguments)
					{
						Core.Write(writer, arg.Expression);
						writer.Write(", ");
					}
				};

			if (operatorToken.Kind() == SyntaxKind.EqualsToken)
			{
				var leftTypeHaxe = TypeProcessor.ConvertType(elementAccess.Expression);

				if (leftTypeHaxe == "haxe.io.Bytes")
					writer.Write("set(");
				else
					writer.Write("SetValue(");

				writeArgs();
				Core.Write(writer, subExpressionOpt);
				writer.Write(")");
			}
			else
				throw new Exception("Unexpected token following an element access expression " + Utility.Descriptor(elementAccess.Parent));

		}

	}
}
