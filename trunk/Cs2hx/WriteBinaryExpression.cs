using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteBinaryExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
		{
			//Check for index assignments, for example dictionary[4] = 3;
			if (expression.Left is ElementAccessExpressionSyntax && expression.OperatorToken.Kind == SyntaxKind.EqualsToken)
			{
				var elementAccess = (ElementAccessExpressionSyntax)expression.Left;

				var typeHaxe = TypeProcessor.ConvertType(Program.GetModel(expression).GetTypeInfo(elementAccess.Expression).ConvertedType);

				if (!typeHaxe.StartsWith("Array<")) //arrays are the only thing haxe allows assignments into via indexing
				{
					WriteIndexingExpression(writer, expression.OperatorToken, expression.Right, elementAccess);
					return;
				}
			}


			//Check for improper nullable access, unless it's assignment or string concat, which work fine.  Note that if they're trying to add nullable number types, it won't catch it since we can't tell if it's string concatentation or addition.  But haxe will fail to build, so it should still be caught easily enough.
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

			if (expression.OperatorToken.Kind == SyntaxKind.PlusEqualsToken || expression.OperatorToken.Kind == SyntaxKind.MinusEqualsToken)
			{
				//Check for event subscription/removal
				var leftSymbol = Program.GetModel(expression).GetSymbolInfo(expression.Left);
				if (leftSymbol.Symbol is EventSymbol)
				{
					Core.Write(writer, expression.Left);
					if (expression.OperatorToken.Kind == SyntaxKind.PlusEqualsToken)
						writer.Write(".Add(");
					else
						writer.Write(".Remove(");
					Core.Write(writer, expression.Right);
					writer.Write(")");
					return;
				}
			}


			if (expression.OperatorToken.Kind == SyntaxKind.AsKeyword)
			{
				var leftStr = Utility.TryGetIdentifier(expression.Left);

				if (leftStr == null)
					throw new Exception("The \"as\" keyword can only be used on simple names.  Declare it as a local variable. " + Utility.Descriptor(expression));

				var typeHaxe = TypeProcessor.ConvertType(expression.Right);

				writer.Write("(Std.is(" + leftStr + ", " + typeHaxe + ") ? cast(" + leftStr + ", " + typeHaxe + ") : null)");

			}
			else if (expression.OperatorToken.Kind == SyntaxKind.IsKeyword)
			{
				writer.Write("Std.is(");
				Core.Write(writer, expression.Left);
				writer.Write(", ");
				writer.Write(TypeProcessor.RemoveGenericArguments(TypeProcessor.ConvertType(expression.Right)));
				writer.Write(")");
			}
			else if (expression.OperatorToken.Kind == SyntaxKind.QuestionQuestionToken)
			{
				writer.Write("Cs2Hx.Coalesce(");
				Core.Write(writer, expression.Left);
				writer.Write(", ");
				Core.Write(writer, expression.Right);
				writer.Write(")");
			}
			else
			{
				Action<ExpressionSyntax, ExpressionSyntax> write = (e, otherSide) =>
					{
						var type = Program.GetModel(expression).GetTypeInfo(e);
                        var otherType = Program.GetModel(expression).GetTypeInfo(otherSide);
						//Check for enums being converted to strings by string concatenation
						if (expression.OperatorToken.Kind == SyntaxKind.PlusToken && type.Type.TypeKind == TypeKind.Enum)
						{
							writer.Write(type.Type.ContainingNamespace.FullNameWithDot().ToLower());
							writer.Write(WriteType.TypeName(type.Type.As<NamedTypeSymbol>()));
							writer.Write(".ToString(");
							Core.Write(writer, e);
							writer.Write(")");
						}
                        else if (expression.OperatorToken.Kind == SyntaxKind.PlusToken && type.Type.SpecialType == SpecialType.System_Char && otherType.Type.SpecialType != SpecialType.System_Char && otherType.Type.SpecialType != SpecialType.System_Int32)
                        {
                            //Chars are integers in haxe, so when string-concatening them we must convert them to strings
                            writer.Write("Cs2Hx.CharToString(");
                            Core.Write(writer, e);
                            writer.Write(")");
                        }
                        else
							Core.Write(writer, e);
					};

				write(expression.Left, expression.Right);
				writer.Write(" ");
				writer.Write(expression.OperatorToken.ToString()); //we can do this since haxe operators work just like C# operators
				writer.Write(" ");
				write(expression.Right, expression.Left);
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

			if (operatorToken.Kind == SyntaxKind.EqualsToken)
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
