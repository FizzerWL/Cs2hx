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
			var model = Program.GetModel(invocationExpression);

			var symbolInfo = model.GetSymbolInfo(invocationExpression);
			var expressionSymbol = model.GetSymbolInfo(invocationExpression.Expression);
			var methodSymbol = symbolInfo.Symbol.As<MethodSymbol>().UnReduce();

			var translateOpt = Translation.GetTranslation(Translation.TranslationType.Method, methodSymbol.Name, methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name, string.Join(" ", methodSymbol.Parameters.ToList().Select(o => o.Type.ToString()))) as Method;
			var memberReferenceExpressionOpt = invocationExpression.Expression as MemberAccessExpressionSyntax;
			var returnTypeHaxe = TypeProcessor.ConvertType(methodSymbol.ReturnType);
			var firstParameter = true;
			
			var extensionNamespace = methodSymbol.IsExtensionMethod ? Translation.ExtensionName(methodSymbol.ContainingType) : null; //null means it's not an extension method, non-null means it is
			string methodName;
			ExpressionSyntax subExpressionOpt;

			if (methodSymbol.ContainingType.Name == "Enum")
			{
				if (methodSymbol.Name == "Parse")
				{
					WriteEnumParse(writer, invocationExpression);
					return;
				}

				if (methodSymbol.Name == "GetValues")
				{
					WriteEnumGetValues(writer, invocationExpression);
					return;
				}
			}

			if (expressionSymbol.Symbol is EventSymbol)
			{
				methodName = "Invoke"; //Would need to append the number of arguments to this to support events.  However, events are not currently supported
			}
			else if (memberReferenceExpressionOpt != null && memberReferenceExpressionOpt.Expression is PredefinedTypeSyntax)
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
					case "TryParse":
						methodName = "TryParse" + TypeProcessor.ConvertType(methodSymbol.ReturnType);
						extensionNamespace = "Cs2Hx";
						break;
					default:
						methodName = methodSymbol.Name;
						extensionNamespace = "Cs2Hx";
						break;
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
			if (extensionNamespace != null && subExpressionOpt != null && model.GetTypeInfo(subExpressionOpt).ConvertedType.ToString() == methodSymbol.ContainingNamespace + "." + methodSymbol.ContainingType.Name)
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

					if (methodSymbol.IsExtensionMethod)
						WriteForEachStatement.CheckWriteEnumerator(writer, subExpressionOpt);
					else
						Core.Write(writer, subExpressionOpt);
				}
			}
			else
			{
				//Check against lowercase toString since it gets replaced with the haxe name before we get here
				if (memberReferenceExpressionOpt != null)
				{
					var memberType = model.GetTypeInfo(memberReferenceExpressionOpt.Expression).Type;
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

					if (methodName == "toString")
					{

						if (memberType.TypeKind == TypeKind.Enum)
						{
							//calling ToString() on an enum forwards to our enum's special ToString method
							writer.Write(WriteType.TypeName((NamedTypeSymbol)memberType));
							writer.Write(".ToString(");
							Core.Write(writer, memberReferenceExpressionOpt.Expression);
							writer.Write(")");

							if (invocationExpression.ArgumentList.Arguments.Count > 0)
								throw new Exception("Enum's ToString detected with parameters.  These are not supported " + Utility.Descriptor(invocationExpression));

							return;
						}

						if (memberTypeHaxe == "Int" || memberTypeHaxe == "Float" || memberTypeHaxe == "Bool" || memberType.TypeKind == TypeKind.TypeParameter)
						{
							//ToString()'s on primitive types get replaced with Std.string
							writer.Write("Std.string(");
							Core.Write(writer, memberReferenceExpressionOpt.Expression);
							writer.Write(")");

							if (invocationExpression.ArgumentList.Arguments.Count > 0)
								throw new Exception("Primitive type's ToString detected with parameters.  These are not supported " + Utility.Descriptor(invocationExpression));

							return; //Skip parameters
						}

					}
				}

				if (subExpressionOpt != null)
					Core.Write(writer, subExpressionOpt);

				if (subExpressionOpt != null && methodName != null)
					writer.Write(".");

				writer.Write(methodName);
				writer.Write("(");
			}


            foreach (var arg in TranslateParameters(translateOpt, SortArguments(methodSymbol, invocationExpression.ArgumentList.Arguments, invocationExpression), invocationExpression))
            {
                if (firstParameter)
                    firstParameter = false;
                else
                    writer.Write(", ");

				
				var isRefField = arg.ArgumentOpt != null
					&& arg.ArgumentOpt.RefOrOutKeyword.Kind != SyntaxKind.None
					&& model.GetSymbolInfo(arg.ArgumentOpt.Expression).Symbol is FieldSymbol;

				if (isRefField)
					writer.Write("new CsRef<" + TypeProcessor.ConvertType(model.GetTypeInfo(arg.ArgumentOpt.Expression).Type) + ">(");

				//When passing an argument by ref or out, leave off the .Value suffix
				if (arg.ArgumentOpt != null && arg.ArgumentOpt.RefOrOutKeyword.Kind != SyntaxKind.None && !isRefField)
					WriteIdentifierName.Go(writer, arg.ArgumentOpt.Expression.As<IdentifierNameSyntax>(), true);
				else if (arg.ArgumentOpt != null)
					WriteForEachStatement.CheckWriteEnumerator(writer, arg.ArgumentOpt.Expression);
				else
					arg.Write(writer);

				if (isRefField)
					writer.Write(")");

				
            }


            writer.Write(")");
		}

		/// <summary>
		/// calls to Enum.Parse get re-written as calls to our special Parse methods on each enum.  We assume the first parameter to Enum.Parse is a a typeof()
		/// </summary>
		private static void WriteEnumParse(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
		{
			var args = invocationExpression.ArgumentList.Arguments;

			if (args.Count < 2 || args.Count > 3)
				throw new Exception("Expected 2-3 args to Enum.Parse");

			if (args.Count == 3 && (!(args[2].Expression is LiteralExpressionSyntax) || args[2].Expression.As<LiteralExpressionSyntax>().ToString() != "false"))
				throw new NotImplementedException("Case-insensitive Enum.Parse is not supported " + Utility.Descriptor(invocationExpression));

			if (!(args[0].Expression is TypeOfExpressionSyntax))
				throw new Exception("Expected a typeof() expression as the first parameter of Enum.Parse " + Utility.Descriptor(invocationExpression));

			var type = WriteType.TypeName((NamedTypeSymbol)Program.GetModel(invocationExpression).GetTypeInfo(args[0].Expression.As<TypeOfExpressionSyntax>().Type).Type);

			writer.Write(type);
			writer.Write(".Parse(");
			Core.Write(writer, args[1].Expression);
			writer.Write(")");
		}

		private static void WriteEnumGetValues(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
		{
			if (!(invocationExpression.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax))
				throw new Exception("Expected a typeof() expression as the first parameter of Enum.GetValues " + Utility.Descriptor(invocationExpression));

			var type = WriteType.TypeName((NamedTypeSymbol)Program.GetModel(invocationExpression).GetTypeInfo(invocationExpression.ArgumentList.Arguments[0].Expression.As<TypeOfExpressionSyntax>().Type).Type);

			writer.Write(type);
			writer.Write(".Values()");
		}


		/// <summary>
		/// If named parameters are used, re-arrange the arguments so that they're in the order defined by the method.
		/// Since we assume it's valid C#, we don't need to check for any error conditions
		/// </summary>
		/// <param name="method"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private static IEnumerable<ArgumentSyntax> SortArguments(MethodSymbol method, SeparatedSyntaxList<ArgumentSyntax> arguments, ExpressionSyntax expressionForErr)
		{
			if (arguments.All(o => o.NameColon == null))
				return arguments; //no named parameters. Return them as-is.

			var ret = new List<ArgumentSyntax>(arguments.Count);

			//First transer any args that don't have named parameters straight over.
			foreach (var arg in arguments)
			{
				if (arg.NameColon != null)
					break;

				ret.Add(arg);
			}

			var namedArgs = arguments.Skip(ret.Count).ToDictionary(o => o.NameColon.Name.Identifier.ValueText, o => o);

			foreach (var param in method.Parameters.ToList().Skip(ret.Count))
			{
				if (namedArgs.ContainsKey(param.Name))
				{
					ret.Add(namedArgs[param.Name]);
					namedArgs.Remove(param.Name);
				}
			}

			if (namedArgs.Count > 0)
				throw new Exception("Named parameters not found: " + string.Join(", ", namedArgs.Keys) + " on " + Utility.Descriptor(expressionForErr));

			return ret;
		}

		private static IEnumerable<TransformedArgument> TranslateParameters(Translations.Translation translateOpt, IEnumerable<ArgumentSyntax> list, InvocationExpressionSyntax invoke)
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
