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
			var symbolInfo = Program.GetModel(invocationExpression).GetSymbolInfo(invocationExpression);
			var methodSymbol = symbolInfo.Symbol.As<MethodSymbol>().UnReduce();

			var translateOpt = Translation.GetTranslation(Translation.TranslationType.Method, methodSymbol.Name, methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name, string.Join(" ", methodSymbol.Parameters.ToList().Select(o => o.Type.ToString()))) as Method;
			var memberReferenceExpressionOpt = invocationExpression.Expression as MemberAccessExpressionSyntax;
			var returnTypeHaxe = TypeProcessor.ConvertType(methodSymbol.ReturnType);
			var firstParameter = true;
			

			var extensionNamespace = methodSymbol.IsExtensionMethod ? Translation.ExtensionName(methodSymbol.ContainingType) : null; //null means it's not an extension method, non-null means it is
			string methodName;
			ExpressionSyntax subExpressionOpt;

			if (memberReferenceExpressionOpt != null && memberReferenceExpressionOpt.Expression is PredefinedTypeSyntax)
			{
				switch (methodSymbol.Name)
				{
					case "Parse":
						var t = TypeProcessor.ConvertType(methodSymbol.ReturnType);

						if (t == "Bool")
						{
							methodName = "ParseBool";
							extensionNamespace = "Cs2Hx";
						}
						else if (t == "Int" || t == "Float")
						{
							methodName = "parse" + t;
							extensionNamespace = "Std";
						}
						else
							throw new Exception("Parse method on " + t + " is not supported.  " + Utility.Descriptor(memberReferenceExpressionOpt));

						break;
					case "IsNaN":
						methodName = "isNaN";
						extensionNamespace = "Math";
						break;
					case "IsInfinity":
						methodName = "IsInfinity";
						extensionNamespace = "Cs2Hx";
						break;
					case "Join":
						methodName = "Join";
						extensionNamespace = "Cs2Hx";
						break;
					case "IsNullOrEmpty":
						methodName = "IsNullOrEmpty";
						extensionNamespace = "Cs2Hx";
						break;
					default:
						throw new Exception(methodSymbol.Name + " is not supported.  " + Utility.Descriptor(invocationExpression));
				}
			}
			else if (translateOpt != null && translateOpt.ReplaceWith != null)
				methodName = translateOpt.ReplaceWith;
			else if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
				methodName = null;
			else
				methodName = OverloadResolver.MethodName(methodSymbol);

			if (translateOpt != null && translateOpt.HasComplexReplaceWith)
			{
				translateOpt.DoComplexReplaceWith(writer, memberReferenceExpressionOpt); //TODO: What if this is null?
				return;
			}

			if (translateOpt != null && translateOpt.SkipExtensionParameter)
				subExpressionOpt = null;
			else if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
				subExpressionOpt = invocationExpression.Expression;
			else if (memberReferenceExpressionOpt != null)
			{
				if (memberReferenceExpressionOpt.Expression is PredefinedTypeSyntax)
					subExpressionOpt = null;
				else
					subExpressionOpt = memberReferenceExpressionOpt.Expression;
			}
			else
				subExpressionOpt = null; //TODO


			//Determine if it's an extension method called in a non-extension way.  In this case, just pretend it's not an extension method
			if (extensionNamespace != null && subExpressionOpt != null && Program.GetModel(subExpressionOpt).GetTypeInfo(subExpressionOpt).ConvertedType.ToString() == methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name)
				extensionNamespace = null;

			if (translateOpt != null && !string.IsNullOrEmpty(translateOpt.ExtensionNamespace))
				extensionNamespace = translateOpt.ExtensionNamespace.SubstringAfterLast('.');
			else if (translateOpt != null && translateOpt.ExtensionNamespace == "")
				extensionNamespace = null;

			if (extensionNamespace != null)
			{
				writer.Write(extensionNamespace);

				if (methodName != null)
				{
					writer.Write(".");
					writer.Write(methodName);
				}

				writer.Write("(");

				if (subExpressionOpt != null)
				{
					firstParameter = false;
					Core.Write(writer, subExpressionOpt);

					if (methodSymbol.IsExtensionMethod)
						WriteForEachStatement.CheckEnumeratorSuffix(writer, subExpressionOpt);
				}
			}
			else
			{
				//Check against lowercase toString since it gets replaced with the haxe name before we get here
				if (memberReferenceExpressionOpt != null)
				{
					var memberType = Program.GetModel(memberReferenceExpressionOpt).GetTypeInfo(memberReferenceExpressionOpt.Expression).Type;
					var memberTypeHaxe = TypeProcessor.ConvertType(memberType);

					//sort calls without any parameters need to get the default sort parameter
					if (methodName == "sort" && invocationExpression.ArgumentList.Arguments.Count == 0)
					{
						Core.Write(writer, memberReferenceExpressionOpt.Expression);
						writer.Write(".sort(");

						switch (memberTypeHaxe)
						{
							case "Array<Int>":
								writer.Write("Cs2Hx.SortInts");
								break;
							case "Array<Float>":
								writer.Write("Cs2Hx.SortFloats");
								break;
							case "Array<String>":
								writer.Write("Cs2Hx.SortStrings");
								break;
							default:
								throw new Exception("Unknown default sort type: " + memberTypeHaxe + ".  " + Utility.Descriptor(invocationExpression));
						}

						writer.Write(")");
						return;
					}

					if (methodName == "toString" && (memberTypeHaxe == "Int" || memberTypeHaxe == "Float" || memberTypeHaxe == "Bool" || memberType.TypeKind == TypeKind.TypeParameter))
					{
						//ToString()'s on primitive types get replaced with Std.string
						writer.Write("Std.string(");
						Core.Write(writer, memberReferenceExpressionOpt.Expression);
						writer.Write(")");

						if (invocationExpression.ArgumentList.Arguments.Count > 0)
							throw new Exception("Primitive type's ToString detected with parameters.  These are not supported in haXe. " + Utility.Descriptor(invocationExpression));

						return; //Skip parameters
					}
				}

				if (subExpressionOpt != null)
					Core.Write(writer, subExpressionOpt);

				if (subExpressionOpt != null && methodName != null)
					writer.Write(".");

				writer.Write(methodName);
				writer.Write("(");
			}


            foreach (var arg in TranslateParameters(translateOpt, invocationExpression.ArgumentList.Arguments, invocationExpression))
            {
                if (firstParameter)
                    firstParameter = false;
                else
                    writer.Write(", ");

				/*
				var isRefField = arg.ArgumentOpt != null 
					&& arg.ArgumentOpt.RefOrOutKeyword != null
					&& Program.GetModel(invocationExpression).GetSymbolInfo(arg.ArgumentOpt.Expression).Symbol is FieldSymbol;

				if (isRefField)
					writer.Write("new CsRef<" + TypeProcessor.ConvertType(Program.GetModel(invocationExpression).GetTypeInfo(arg.ArgumentOpt.Expression).Type) + ">(");*/

				arg.Write(writer);

				/*
				if (isRefField)
					writer.Write(")");
				 */ 

				if (arg.ArgumentOpt != null)
					WriteForEachStatement.CheckEnumeratorSuffix(writer, arg.ArgumentOpt.Expression);
            }


            writer.Write(")");
		}

		private static IEnumerable<TransformedArgument> TranslateParameters(Translations.Translation translateOpt, SeparatedSyntaxList<ArgumentSyntax> list, InvocationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list.Select(o => new TransformedArgument(o));
			else if (translateOpt is Translations.Method)
				return translateOpt.As<Translations.Method>().TranslateParameters(list, invoke.Expression);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}


    }
}
