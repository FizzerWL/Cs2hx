using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Cs2hx
{
	static class WriteBinaryExpression
	{
		public static void Go(HaxeWriter writer, BinaryExpressionSyntax expression)
        {
            //check for invocation of overloaded operator
            var symbolInfo = Program.GetModel(expression).GetSymbolInfo(expression);
            if (symbolInfo.Symbol != null && symbolInfo.Symbol is IMethodSymbol)
            {
                var method = (IMethodSymbol)symbolInfo.Symbol;
                var type = Program.GetModel(expression).GetTypeInfo(expression).Type;
                if (method.Name.StartsWith("op_") && !type.ContainingNamespace.FullNameWithDot().StartsWith("System."))
                {
                    WriteOverloadedOperatorInvocation(writer, expression, method, type);
                    return;
                }
            }


            //Check for integer division.  Integer division is handled automatically in C#, but must be explicit in haxe.
            if (expression.OperatorToken.Kind() == SyntaxKind.SlashToken && Program.GetModel(expression).GetTypeInfo(expression).Type.SpecialType == SpecialType.System_Int32)
            {
                //If parent is a cast to int, skip this step.  This isn't necessary for correctness, but it makes cleaner code.
                var castIsExplicit = expression.Parent is ParenthesizedExpressionSyntax && expression.Parent.Parent is CastExpressionSyntax && expression.Parent.Parent.As<CastExpressionSyntax>().Type.ToString() == "int";

                if (!castIsExplicit)
                {
                    writer.Write("Std.int(");
                    Go(writer, expression.Left, expression.OperatorToken, expression.Right);
                    writer.Write(")");
                    return;
                }
            }



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
                var leftType = Program.GetModel(left).GetTypeInfo(left);
                var rightType = Program.GetModel(right).GetTypeInfo(right);

                if ((operatorToken.Kind() == SyntaxKind.EqualsEqualsToken || operatorToken.Kind() == SyntaxKind.ExclamationEqualsToken)
                    && leftType.ConvertedType.SpecialType == SpecialType.System_Boolean
                    && rightType.ConvertedType.SpecialType == SpecialType.System_Boolean)
                {
                    //Anytime we == or != booleans, we need to take special care when dealing with the js target.  haxe seems to leave some variables as undefined, which works fine as booleans in most comparisons, but not when comparing against each other such as "x == false"
                    if (operatorToken.Kind() == SyntaxKind.ExclamationEqualsToken)
                        writer.Write("!");
                    writer.Write("Cs2Hx.BoolCompare(");
                    Core.Write(writer, left);
                    writer.Write(", ");
                    Core.Write(writer, right);
                    writer.Write(")");
                    return;
                }

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
                        else if (operatorToken.Kind() == SyntaxKind.PlusToken && !(e is BinaryExpressionSyntax) && type.Type.SpecialType == SpecialType.System_String && CouldBeNullString(Program.GetModel(e), e))
                        {
                            //In .net, concatenating a null string does not alter the output. However, in haxe's js target, it produces the "null" string. To counter this, we must check non-const strings.
                            writer.Write("system.Cs2Hx.NullCheck(");
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
                {
                    var symbol = Program.GetModel(elementAccess).GetSymbolInfo(elementAccess).Symbol.OriginalDefinition.As<IPropertySymbol>();
                    var types = string.Join("", symbol.Parameters.ToArray().Select(o => "_" + o.Type.Name));
                    writer.Write("SetValue" + types + "(");
                }

				writeArgs();
				Core.Write(writer, subExpressionOpt);
				writer.Write(")");
			}
			else
				throw new Exception("Unexpected token following an element access expression " + Utility.Descriptor(elementAccess.Parent));

		}


        private static void WriteOverloadedOperatorInvocation(HaxeWriter writer, BinaryExpressionSyntax expression, IMethodSymbol method, ITypeSymbol type)
        {
            writer.Write(type.ContainingNamespace.FullNameWithDot().ToLower());
            writer.Write(type.Name);
            writer.Write(".");
            writer.Write(OverloadResolver.MethodName(method));
            writer.Write("(");
            Core.Write(writer, expression.Left);
            writer.Write(", ");
            Core.Write(writer, expression.Right);
            writer.Write(")");
        }


        private static bool CouldBeNullString(SemanticModel model, ExpressionSyntax e)
        {
            if (model.GetConstantValue(e).HasValue)
                return false; //constants are never null

            //For in-line conditions, just recurse on both results.
            var cond = e as ConditionalExpressionSyntax;
            if (cond != null)
                return CouldBeNullString(model, cond.WhenTrue) || CouldBeNullString(model, cond.WhenFalse);

            var paren = e as ParenthesizedExpressionSyntax;
            if (paren != null)
                return CouldBeNullString(model, paren.Expression);

            var invoke = e as InvocationExpressionSyntax;
            if (invoke != null)
            {
                var methodSymbol = model.GetSymbolInfo(invoke).Symbol;
                //Hard-code some well-known functions as an optimization
                if (methodSymbol.Name == "HtmlEncode" && methodSymbol.ContainingNamespace.FullName() == "System.Web")
                    return false;
                if (methodSymbol.Name == "ToString")
                    return false;
            }

            return true;
        }
    }
}
