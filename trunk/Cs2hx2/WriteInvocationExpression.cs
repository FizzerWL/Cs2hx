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

			var extensionNamespace = methodSymbol.IsExtensionMethod ? Translation.ExtensionName(methodSymbol.ContainingType) : null; //null means it's not an extension method, non-null means it is

			if (invocationExpression.Expression is GenericNameSyntax)
			{
				//Write it without the generic types. haxe will have to figure them out itself
				writer.Write(invocationExpression.Expression.As<GenericNameSyntax>().Identifier.ValueText);
			}
			else if (!(invocationExpression.Expression is MemberAccessExpressionSyntax))
			{
				extensionNamespace = null;
				Core.Write(writer, invocationExpression.Expression);
			}
			else
			{
				var memberReferenceExpression = invocationExpression.Expression.As<MemberAccessExpressionSyntax>();
				var expressionTypeOpt = TypeState.Instance.GetModel(invocationExpression).GetTypeInfo(memberReferenceExpression.Expression).ConvertedType;

				//Determine if it's an extension method called in a non-extension way.  In this case, just pretend it's not an extension method
				if (extensionNamespace != null && expressionTypeOpt != null && expressionTypeOpt.ToString() == methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name)
					extensionNamespace = null;


				//Determine if we need to transform the method name
				string methodName;
				if (translateOpt == null || translateOpt.ReplaceWith == null)
					methodName = methodSymbol.Name;
				else
					methodName = translateOpt.ReplaceWith;

				if (translateOpt != null && translateOpt.ExtensionNamespace != null)
					extensionNamespace = translateOpt.ExtensionNamespace.SubstringAfterLast('.');

				if (translateOpt != null && translateOpt.HasComplexReplaceWith)
				{
					translateOpt.DoComplexReplaceWith(writer, memberReferenceExpression);
					return;
				}
				else if (extensionNamespace != null)
				{
					writer.Write(extensionNamespace);
					writer.Write(".");
					writer.Write(methodName);
					writer.Write("(");

					if (translateOpt == null || !translateOpt.SkipExtensionParameter)
						Core.Write(writer, memberReferenceExpression.Expression);
				}
				else
				{
					
					if (memberReferenceExpression.Expression is PredefinedTypeSyntax)
					{
						switch (methodName)
						{
							case "Parse":
								var t = TypeProcessor.ConvertType((TypeSymbol)expressionTypeOpt);

								if (t == "Bool")
									writer.Write("HaxeUtility.ParseBool");
								else if (t == "Int" || t == "Float")
									writer.Write("Std.parse" + t);
								else
									throw new Exception("Parse method on " + t + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));

								break;
							case "IsNaN":
								writer.Write("Math.isNaN");
								break;
							case "IsInfinity":
								writer.Write("Cs2Hx.IsInfinity");
								break;
							case "Join":
								writer.Write("Cs2Hx.Join");
								break;
							default:
								throw new Exception(methodName + " is not supported.  " + Utility.Descriptor(memberReferenceExpression));
						}
					}
					else
					{
						if (expressionTypeOpt != null)
						{
							var varType = TypeProcessor.ConvertType((TypeSymbol)expressionTypeOpt);

							//Check against lowercase toString since it gets replaced with the haxe name before we get here
							if (methodName == "toString" && (varType == "Int" || varType == "Float"))
							{
								//ToString()'s on primitive types get replaced with Std.string
								writer.Write("Std.string(");
								Core.Write(writer, memberReferenceExpression.Expression);
								writer.Write(")");

								if (invocationExpression.ArgumentList.Arguments.Count > 0)
									throw new Exception("Primitive type's ToString detected with parameters.  These are not supported in haXe. " + Utility.Descriptor(invocationExpression));

								return; //Skip any parameters
							}
						}
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
						//else
						{
							Core.Write(writer, memberReferenceExpression.Expression);
							writer.Write(".");
							writer.Write(methodName);
						}
					}
				}
			}

			//extension methods already had the first argument written above, so skip the open paren and always render the first comma
            if (extensionNamespace == null)
                writer.Write("(");
            var firstArg = extensionNamespace == null || (translateOpt != null && translateOpt.SkipExtensionParameter); 

            foreach (var arg in TranslateParameters(translateOpt, invocationExpression.ArgumentList.Arguments, invocationExpression))
            {
                if (firstArg)
                    firstArg = false;
                else
                    writer.Write(", ");

				arg.Write(writer);
            }


            writer.Write(")");
		}

		private static IEnumerable<ExpressionOrString> TranslateParameters(Translations.Translation translateOpt, SeparatedSyntaxList<ArgumentSyntax> list, InvocationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list.Select(o => new ExpressionOrString { Expression = o.Expression });
			else if (translateOpt is Translations.Method)
				return translateOpt.As<Translations.Method>().TranslateParameters(list.Select(o => o.Expression), invoke.Expression);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}


    }
}
