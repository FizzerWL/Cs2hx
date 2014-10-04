using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cs2hx.Translations;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteObjectCreationExpression
	{
		public static void Go(HaxeWriter writer, ObjectCreationExpressionSyntax expression)
		{
			if (expression.ArgumentList == null)
				throw new Exception("Types must be initialized with parenthesis. Object initialization syntax is not supported. " + Utility.Descriptor(expression));

			var model = Program.GetModel(expression);
			var type = model.GetTypeInfo(expression).Type;

			if (type.SpecialType == Roslyn.Compilers.SpecialType.System_Object)
			{
				//new object() results in the CsObject type being made.  This is only really useful for locking
				writer.Write("new CsObject()");
			}
			else
			{
				var methodSymbol = model.GetSymbolInfo(expression).Symbol.As<MethodSymbol>();

				var translateOpt = MethodTranslation.Get(methodSymbol);
				

				writer.Write("new ");
				writer.Write(TypeProcessor.ConvertType(expression.Type));
				writer.Write("(");

				bool first = true;
				foreach (var param in TranslateParameters(translateOpt, WriteInvocationExpression.SortArguments(methodSymbol, expression.ArgumentList.Arguments, expression), expression))
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
