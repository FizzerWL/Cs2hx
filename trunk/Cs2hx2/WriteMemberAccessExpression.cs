using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cs2hx.Translations;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteMemberAccessExpression
	{
		public static void Go(HaxeWriter writer, MemberAccessExpressionSyntax expression)
		{
			var memberName = expression.Name.Identifier.ValueText;
			var type = TypeState.Instance.GetModel(expression).GetTypeInfo(expression.Expression).ConvertedType;
			var typeStr = TypeProcessor.GenericTypeName(type);

			if (expression.Expression is PredefinedTypeSyntax && (memberName == "MaxValue" || memberName == "MinValue" || memberName == "Empty"))
			{
				//Support int.MaxValue/int.MaxValue/etc
				if (memberName == "Empty" && typeStr == "System.String")
					writer.Write("\"\"");
				else if (memberName == "MinValue" && typeStr == "System.Double")
					writer.Write("-1.7976931348623e+308");  //We change double.MinValue since haXe can't deal with the real MinValue.  Any checks against this should use <= in place of ==
				else if (memberName == "MaxValue" && typeStr == "System.Int64")
					writer.Write("999900000000000000"); //We change long.MaxValue since haXe can't deal with the real MaxValue. Any checks against this should use >= in place of ==
				else
					writer.Write(System.Type.GetType(typeStr).GetField(memberName).GetValue(null).ToString());
			}
			else 
			{

				var translate = Translation.GetTranslation(Translation.TranslationType.Property, memberName, TypeProcessor.MatchString(typeStr)) as Property;
				if (translate != null)
					memberName = translate.ReplaceWith;

				Core.Write(writer, expression.Expression);
				writer.Write(".");
				writer.Write(memberName);

				if (expression.Name is GenericNameSyntax)
				{
					var gen = expression.Name.As<GenericNameSyntax>();

					writer.Write("<");

					bool first = true;
					foreach (var g in gen.TypeArgumentList.Arguments)
					{
						if (first)
							first = false;
						else
							writer.Write(", ");

						writer.Write(":" + TypeProcessor.ConvertType(g));
					}

					writer.Write(">");
				}
			}
		}
	}
}
