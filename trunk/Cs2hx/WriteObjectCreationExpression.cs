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
			var type = TypeState.Instance.GetModel(expression).GetTypeInfo(expression).ConvertedType;

			var translateOpt = Translation.GetTranslation(Translation.TranslationType.Method, ".ctor", TypeProcessor.MatchString(TypeProcessor.GenericTypeName(type))) as Method;

			writer.Write("new ");
			writer.Write(TypeProcessor.ConvertType(expression.Type));
			writer.Write("(");

			bool first = true;
			foreach (var param in TranslateParameters(translateOpt, expression.ArgumentList.Arguments, expression))
			{
				if (first)
					first = false;
				else
					writer.Write(", ");

				param.Write(writer);
			}

			writer.Write(")");
		}

		private static IEnumerable<TransformedArgument> TranslateParameters(Translation translateOpt, SeparatedSyntaxList<ArgumentSyntax> list, ObjectCreationExpressionSyntax invoke)
		{
			if (translateOpt == null)
				return list.Select(o => new TransformedArgument(o));
			else if (translateOpt is Method)
				return translateOpt.As<Method>().TranslateParameters(list, invoke);
			else
				throw new Exception("Need handler for " + translateOpt.GetType().Name);
		}

	}
}
