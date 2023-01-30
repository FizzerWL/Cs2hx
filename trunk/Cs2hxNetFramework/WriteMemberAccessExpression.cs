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
	static class WriteMemberAccessExpression
	{
		public static void Go(HaxeWriter writer, MemberAccessExpressionSyntax expression)
		{
			var model = Program.GetModel(expression);

			var memberName = expression.Name.Identifier.ValueText;
			var type = model.GetTypeInfo(expression.Expression).ConvertedType;
			var typeStr = TypeProcessor.GenericTypeName(type);

			if (expression.Expression is PredefinedTypeSyntax)
			{
				//Support int.MaxValue/int.MaxValue/etc. We change MinValue/MaxValue for some types since haXe can't deal with the real MinValue (it's even stricter when compiling to java).  Any checks against this should use <= in place of ==
				if (memberName == "Empty" && typeStr == "System.String")
					writer.Write("\"\"");
				else if (memberName == "MaxValue" && typeStr == "System.Double")
					writer.Write("3.4028235e+54");
				else if (memberName == "MinValue" && typeStr == "System.Double")
					writer.Write("-3.4028235e+54");
				else if (memberName == "MaxValue" && typeStr == "System.Int64")
					writer.Write("999900000000000000");
				else if (memberName == "NaN")
					writer.Write("Math.NaN");
				else
				{
					var val = System.Type.GetType(typeStr).GetField(memberName).GetValue(null);
					if (val is string)
						writer.Write("\"" + val + "\"");
					else
						writer.Write(val.ToString());
				}
			}
			else 
			{

				var translate = PropertyTranslation.Get(typeStr, memberName);

                if (translate != null && translate.ExtensionNamespace != null)
                {
                    writer.Write(translate.ExtensionNamespace);
                    writer.Write(".");
                    writer.Write(translate.ReplaceWith);
                    writer.Write("(");

                    if (!translate.SkipExtensionParameter)
                        WriteMember(writer, expression.Expression);

                    writer.Write(")");
                    return;
                }

                if (translate != null)
                {
                    memberName = translate.ReplaceWith;

                }

				if (type != null) //if type is null, then we're just a namespace.  We can ignore these.
				{
					WriteMember(writer, expression.Expression);
					writer.Write(".");
				}

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

						writer.Write(TypeProcessor.ConvertTypeWithColon(g));
					}

					writer.Write(">");
				}
			}
		}

		public static void WriteMember(HaxeWriter writer, ExpressionSyntax expression)
		{
			var symbol = Program.GetModel(expression).GetSymbolInfo(expression).Symbol;
			if (symbol is INamedTypeSymbol)
			{
				var translateOpt = TypeTranslation.Get(symbol.ContainingNamespace.FullNameWithDot() + symbol.Name);

				if (translateOpt != null)
					writer.Write(translateOpt.ReplaceWith);
				else
					writer.Write(symbol.ContainingNamespace.FullNameWithDot().ToLower() + WriteType.TypeName(symbol.As<INamedTypeSymbol>()));
			}
			else
				Core.Write(writer, expression);

			
		}
	}
}
