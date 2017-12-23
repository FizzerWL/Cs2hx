using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cs2hx.Translations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
    static class WriteInvocationExpression
    {
        public static void Go(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
        {
			var model = Program.GetModel(invocationExpression);

			var symbolInfo = model.GetSymbolInfo(invocationExpression);
			var expressionSymbol = model.GetSymbolInfo(invocationExpression.Expression);
			var methodSymbol = symbolInfo.Symbol.OriginalDefinition.As<IMethodSymbol>().UnReduce();

			var translateOpt = MethodTranslation.Get(symbolInfo.Symbol.As<IMethodSymbol>());
			var memberReferenceExpressionOpt = invocationExpression.Expression as MemberAccessExpressionSyntax;
			var returnTypeHaxe = TypeProcessor.ConvertType(methodSymbol.ReturnType);
			var firstParameter = true;
			
			var extensionNamespace = methodSymbol.IsExtensionMethod ? methodSymbol.ContainingNamespace.FullNameWithDot().ToLower() + methodSymbol.ContainingType.Name : null; //null means it's not an extension method, non-null means it is
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

			if (expressionSymbol.Symbol is IEventSymbol)
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
						methodName = "TryParse" + TypeProcessor.ConvertType(methodSymbol.Parameters[1].Type);
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
				extensionNamespace = translateOpt.ExtensionNamespace;
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
						WriteForEachStatement.CheckWriteEnumerator(writer, subExpressionOpt, true);
					else
						Core.Write(writer, subExpressionOpt);
				}
			}
			else
			{
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

					//Check against lowercase toString since it gets replaced with the haxe name before we get here
					if (methodName == "toString")
					{

						if (memberType.TypeKind == TypeKind.Enum)
						{
							//calling ToString() on an enum forwards to our enum's special ToString method
							writer.Write(memberType.ContainingNamespace.FullNameWithDot().ToLower());
							writer.Write(WriteType.TypeName((INamedTypeSymbol)memberType));
							writer.Write(".ToString(");
							Core.Write(writer, memberReferenceExpressionOpt.Expression);
							writer.Write(")");

							if (invocationExpression.ArgumentList.Arguments.Count > 0)
								throw new Exception("Enum's ToString detected with parameters.  These are not supported " + Utility.Descriptor(invocationExpression));

							return;
						}

                        if (memberType.SpecialType == SpecialType.System_Char)
                        {
                            writer.Write("Cs2Hx.CharToString(");
                            Core.Write(writer, memberReferenceExpressionOpt.Expression);
                            writer.Write(")");

                            if (invocationExpression.ArgumentList.Arguments.Count > 0)
                                throw new Exception("Char's ToString detected with parameters.  These are not supported " + Utility.Descriptor(invocationExpression));

                            return;

                        }

						if (memberTypeHaxe == "Int" || memberTypeHaxe == "Float" || memberTypeHaxe == "Bool" || memberType.TypeKind == TypeKind.TypeParameter)
						{
							//ToString()'s on primitive types get replaced with Std.string
							writer.Write("Std.string(");

							if (memberReferenceExpressionOpt.Expression is ParenthesizedExpressionSyntax)
								Core.Write(writer, memberReferenceExpressionOpt.Expression.As<ParenthesizedExpressionSyntax>().Expression); //eat a set of parenthesis, just to make the output code a bit nicer.
							else 
								Core.Write(writer, memberReferenceExpressionOpt.Expression);

							writer.Write(")");

							if (invocationExpression.ArgumentList.Arguments.Count > 0)
								throw new Exception("Primitive type's ToString detected with parameters.  These are not supported " + Utility.Descriptor(invocationExpression));

							return; //Skip parameters
						}

					}
				}

				if (subExpressionOpt != null)
					WriteMemberAccessExpression.WriteMember(writer, subExpressionOpt);

				if (subExpressionOpt != null && methodName != null)
					writer.Write(".");

				writer.Write(methodName);
				writer.Write("(");
			}

            var prms = TranslateParameters(translateOpt, SortArguments(methodSymbol, invocationExpression.ArgumentList.Arguments, invocationExpression), invocationExpression).ToList();

            //If we invoke a method with type parameters that aren't used in the argument list, the haxe function won't have a way to see what args were used. To give it a way, add those as parameters at the end
            foreach (var typePrm in Utility.PassTypeArgsToMethod(methodSymbol))
            {
                var name = invocationExpression.Expression.DescendantNodesAndSelf().OfType<GenericNameSyntax>().ToList();
                if (name.Count > 0 && name.Single().TypeArgumentList.Arguments.Count > 0)
                {
                    var typePrmIndex = methodSymbol.TypeParameters.IndexOf(methodSymbol.TypeParameters.Single(o => o == typePrm));
                    var genericVar = name.Single().TypeArgumentList.Arguments.ElementAt(typePrmIndex);
                    prms.Add(new TransformedArgument(TypeProcessor.ConvertType(genericVar)));
                }
            }


            bool inParams = false;
            foreach (var arg in prms)
            {
                if (firstParameter)
                    firstParameter = false;
                else
                    writer.Write(", ");

				if (!inParams && IsParamsArgument(invocationExpression, arg.ArgumentOpt, methodSymbol) && TypeProcessor.ConvertType(model.GetTypeInfo(arg.ArgumentOpt.Expression).Type).StartsWith("Array<") == false)
				{
					inParams = true;
					writer.Write("[ ");
				}


				if (arg.ArgumentOpt != null
					&& arg.ArgumentOpt.RefOrOutKeyword.Kind() != SyntaxKind.None
					&& model.GetSymbolInfo(arg.ArgumentOpt.Expression).Symbol is IFieldSymbol)
					throw new Exception("ref/out cannot reference fields, only local variables.  Consider using ref/out on a local variable and then assigning it into the field. " + Utility.Descriptor(invocationExpression));


				//When passing an argument by ref or out, leave off the .Value suffix
				if (arg.ArgumentOpt != null && arg.ArgumentOpt.RefOrOutKeyword.Kind() != SyntaxKind.None)
					WriteIdentifierName.Go(writer, arg.ArgumentOpt.Expression.As<IdentifierNameSyntax>(), true);
				else if (arg.ArgumentOpt != null)
					WriteForEachStatement.CheckWriteEnumerator(writer, arg.ArgumentOpt.Expression, false);
				else
					arg.Write(writer);

            }

			if (inParams)
				writer.Write(" ]");


            writer.Write(")");
		}

		private static bool IsParamsArgument(InvocationExpressionSyntax invocationExpression, ArgumentSyntax argumentOpt, IMethodSymbol methodSymbol)
		{
			if (argumentOpt == null)
				return false;

			if (invocationExpression.ArgumentList.Arguments.Any(o => o.NameColon != null))
				return false; //params cannot be used with named arguments

			int i = invocationExpression.ArgumentList.Arguments.IndexOf(argumentOpt);
			return methodSymbol.Parameters.ElementAt(i).IsParams;
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

			var type = Program.GetModel(invocationExpression).GetTypeInfo(args[0].Expression.As<TypeOfExpressionSyntax>().Type).Type;
			writer.Write(type.ContainingNamespace.FullNameWithDot().ToLower());
			writer.Write(WriteType.TypeName((INamedTypeSymbol)type));
			writer.Write(".Parse(");
			Core.Write(writer, args[1].Expression);
			writer.Write(")");
		}

		private static void WriteEnumGetValues(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
		{
			if (!(invocationExpression.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax))
				throw new Exception("Expected a typeof() expression as the first parameter of Enum.GetValues " + Utility.Descriptor(invocationExpression));

			var type = Program.GetModel(invocationExpression).GetTypeInfo(invocationExpression.ArgumentList.Arguments[0].Expression.As<TypeOfExpressionSyntax>().Type).Type;

			writer.Write(type.ContainingNamespace.FullNameWithDot().ToLower());
			writer.Write(WriteType.TypeName((INamedTypeSymbol)type));
			writer.Write(".Values()");
		}


		/// <summary>
		/// If named parameters are used, re-arrange the arguments so that they're in the order defined by the method.
		/// Since we assume it's valid C#, we don't need to check for any error conditions
		/// </summary>
		/// <param name="method"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static IEnumerable<TransformedArgument> SortArguments(IMethodSymbol method, IEnumerable<ArgumentSyntax> arguments, ExpressionSyntax expressionForErr)
		{
			if (arguments.All(o => o.NameColon == null))
				return arguments.Select(o => new TransformedArgument(o)); //no named parameters. Return them as-is.

			var ret = new List<TransformedArgument>(arguments.Count());

			//First transer any args that don't have named parameters straight over.
			foreach (var arg in arguments)
			{
				if (arg.NameColon != null)
					break;

				ret.Add(new TransformedArgument(arg));
			}

			var namedArgs = arguments.Skip(ret.Count).ToDictionary(o => o.NameColon.Name.Identifier.ValueText, o => o);

            var prms = method.Parameters.ToList().Skip(ret.Count).ToList();
			foreach (var param in prms)
			{
				if (namedArgs.ContainsKey(param.Name))
				{
					ret.Add(new TransformedArgument(namedArgs[param.Name]));
					namedArgs.Remove(param.Name);
				}
                else if (namedArgs.Count > 0)
                {
                    ret.Add(new TransformedArgument(WriteLiteralExpression.FromObject(param.ExplicitDefaultValue)));
                }
			}

			if (namedArgs.Count > 0)
				throw new Exception("Named parameters not found: " + string.Join(", ", namedArgs.Keys) + " on " + Utility.Descriptor(expressionForErr));

			return ret;
		}

		private static IEnumerable<TransformedArgument> TranslateParameters(MethodTranslation translateOpt, IEnumerable<TransformedArgument> list, InvocationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list;
			else if (translateOpt is Translations.MethodTranslation)
				return translateOpt.As<Translations.MethodTranslation>().TranslateParameters(list, invoke.Expression);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}


    }
}
