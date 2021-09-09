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

            if (type.SpecialType == SpecialType.System_DateTime && expression.ArgumentList.Arguments.Count == 1)
                throw new Exception("You cannot use the DateTime constructor with one argument (ticks).  .net Ticks and Haxe Ticks have different meanings, so this would result in problems. " + Utility.Descriptor(expression));

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

                var convertedType = TypeProcessor.ConvertType(expression.Type);

                if (convertedType == "String")
                {
                    //Normally, writing "new String(" in C# will fall under the above check which calls Cs2Hx.NewString.  However, if a translation changes a type into a string, such as with guids, it falls here.  It's important not to ever write "new String(" in haxe since that makes copies of strings which don't compare properly with ==.  So just embed the string straight.
                    Core.Write(writer, expression.ArgumentList.Arguments.Single().Expression);
                }
                else
                {

                    writer.Write("new ");
                    writer.Write(convertedType);
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
