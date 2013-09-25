using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cs2hx.Translations;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class WriteInvocationExpression
    {
        public static void Go(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
        {

			var symbolInfo = TypeState.Instance.GetModel(invocationExpression).GetSymbolInfo(invocationExpression);
			var methodSymbol = symbolInfo.Symbol.As<MethodSymbol>().UnReduce();

			

			var translateOpt = Translation.GetTranslation(Translation.TranslationType.Method, methodSymbol.Name, methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name) as Method;

			var isExtensionMethod = methodSymbol.IsExtensionMethod;

			if (!(invocationExpression.Expression is MemberAccessExpressionSyntax))
			{
				isExtensionMethod = false;
				Core.Write(writer, invocationExpression.Expression);
			}
			else
			{
				var memberReferenceExpression = invocationExpression.Expression.As<MemberAccessExpressionSyntax>();

				if (isExtensionMethod)
				{
					writer.Write(Translation.ExtensionName(methodSymbol.ContainingType));
					writer.Write(".");
					writer.Write(methodSymbol.Name);
					writer.Write("(");
					Core.Write(writer, memberReferenceExpression.Expression);
				}
				else
				{
					//TODO
					//if (memberReferenceExpression.TargetObject is TypeReferenceExpression)
					//{
					//	switch (methodName)
					//	{
					//		case "Parse":
					//			var t = ConvertRawType(memberReferenceExpression.TargetObject.As<TypeReferenceExpression>().TypeReference);
					//			if (t == null)
					//				throw new Exception("Could not identify Parse method at " + Utility.Descriptor(memberReferenceExpression));
					//			if (t == "Bool")
					//			{
					//				writer.Write("HaxeUtility.ParseBool");
					//			}
					//			else if (t == "Int" || t == "Float")
					//			{
					//				writer.Write("Std.parse" + t);
					//			}
					//			else
					//				throw new Exception("Parse method on " + t + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));

					//			break;
					//		case "IsNaN":
					//			writer.Write("Math.isNaN");
					//			break;
					//		case "IsInfinity":
					//			writer.Write("Cs2Hx.IsInfinity");
					//			break;
					//		case "Join":
					//			writer.Write("Cs2Hx.Join");
					//			break;
					//		default:
					//			throw new Exception(methodName + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));
					//	}
					//}
					//else
					{
						//string varType;
						//TypeReference type;
						//if (memberReferenceExpression.TargetObject is IdentifierExpression && Utility.TryFindType(memberReferenceExpression.TargetObject.As<IdentifierExpression>(), out type))
						//	varType = ConvertRawType(type);
						//else
						//	varType = null;

						//if (methodName.Equals("ToString", StringComparison.OrdinalIgnoreCase) && (varType == "Int" || varType == "Float"))
						//{
						//	//ToString()'s on primitive types get replaced with Std.string
						//	writer.Write("Std.string(");
						//	WriteStatement(writer, memberReferenceExpression.TargetObject);
						//	writer.Write(")");

						//	if (invocationExpression.Arguments.Count > 0)
						//		throw new Exception("Primitive type's ToString detected with parameters.  These are not supported in haXe. " + Utility.Descriptor(invocationExpression));

						//	return; //Skip any parameters
						//}
						//else if (methodName == "sort" && invocationExpression.ArgumentList.Arguments.Count == 0)
						//{
						//	//Sorts without parameters need to get the default sort function added
						//	Core.Write(writer, memberReferenceExpression.Expression);
						//	writer.Write(".");

						//	switch (varType)
						//	{
						//		case "Array<Int>":
						//			writer.Write("sort(Cs2Hx.SortInts)");
						//			break;
						//		case "Array<Float>":
						//			writer.Write("sort(Cs2Hx.SortFloats)");
						//			break;
						//		default:
						//			throw new Exception("Unknown default sort type: " + varType + ".  " + Utility.Descriptor(invocationExpression));
						//	}

						//	return;
						//}
						//else if (methodName == "split" && varType == "String" && invocationExpression.Arguments.Count == 1 && invocationExpression.Arguments.Single() is PrimitiveExpression)
						//{
						//	//C# split takes a char, but haXe split takes a string.
						//	Core.Write(writer, memberReferenceExpression.Expression);
						//	writer.Write(".split(");
						//	writer.Write(invocationExpression.Arguments.Single().As<PrimitiveExpression>().StringValue);
						//	writer.Write(")");
						//	return;
						//}
						//else if (methodName == "As" && memberReferenceExpression.TypeArguments.Count == 1)
						//{
						//	var castTo = this.ConvertRawType(memberReferenceExpression.TypeArguments.Single()) ?? "Dynamic";

						//	writer.Write("cast(");
						//	Core.Write(writer, memberReferenceExpression.Expression);
						//	writer.Write(", ");
						//	writer.Write(castTo);
						//	writer.Write(")");
						//	return;
						//}
						//else
						{
							Core.Write(writer, invocationExpression.Expression);
						}
					}
				}
			}

            if (!isExtensionMethod)
                writer.Write("(");

            var firstArg = !isExtensionMethod;
            foreach (var arg in TranslateParameters(translateOpt, invocationExpression.ArgumentList.Arguments, invocationExpression))
            {
                if (firstArg)
                    firstArg = false;
                else
                    writer.Write(", ");

				Core.Write(writer, arg);
            }


            writer.Write(")");
		}

		private static IEnumerable<ExpressionSyntax> TranslateParameters(Translations.Translation translateOpt, SeparatedSyntaxList<ArgumentSyntax> list, InvocationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list.Select(o => o.Expression);
			else if (translateOpt is Translations.Method)
				return translateOpt.As<Translations.Method>().TranslateParameters(list.Select(o => o.Expression), invoke);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}


    }
}
