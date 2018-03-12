using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cs2hx.Translations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	static class WriteObjectCreationExpression
	{
		public static void Go(HaxeWriter writer, ObjectCreationExpressionSyntax expression)
		{
			if (expression.ArgumentList == null || expression.Initializer != null)
				throw new Exception("Object initialization syntax is not supported. " + Utility.Descriptor(expression));

			var model = Program.GetModel(expression);
			var type = model.GetTypeInfo(expression).Type;
            var methodSymbolUntyped = model.GetSymbolInfo(expression).Symbol;
            if (methodSymbolUntyped == null)
                throw new Exception("methodSymbolUntyped is null");
            var methodSymbol = (IMethodSymbol)methodSymbolUntyped;

            if (type.SpecialType == SpecialType.System_Object)
            {
                //new object() results in the CsObject type being made.  This is only really useful for locking
                writer.Write("new CsObject()");
            }
            else if (type.SpecialType == SpecialType.System_String)
            {
                //new String()
                writer.Write("Cs2Hx.NewString(");
                bool first = true;
                foreach (var param in WriteInvocationExpression.SortArguments(methodSymbol, expression.ArgumentList.Arguments, expression, false))
                {
                    if (first)
                        first = false;
                    else
                        writer.Write(", ");

                    param.Write(writer);
                }
                writer.Write(")");
            }
            else
            {
                var translateOpt = MethodTranslation.Get(methodSymbol);


                writer.Write("new ");
                writer.Write(TypeProcessor.ConvertType(expression.Type));
                writer.Write("(");

                bool first = true;
                foreach (var param in TranslateParameters(translateOpt, WriteInvocationExpression.SortArguments(methodSymbol, expression.ArgumentList.Arguments, expression, false), expression))
                {
                    if (first)
                        first = false;
                    else
                        writer.Write(", ");

                    param.Write(writer);
                }

                writer.Write(")");
            }
		}

		private static IEnumerable<TransformedArgument> TranslateParameters(MethodTranslation translateOpt, IEnumerable<TransformedArgument> list, ObjectCreationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list;
			else if (translateOpt is MethodTranslation)
				return translateOpt.As<MethodTranslation>().TranslateParameters(list, invoke);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}

	}
}
