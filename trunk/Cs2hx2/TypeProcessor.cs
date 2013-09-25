using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class TypeProcessor
    {
        public static string DefaultValue(TypeSyntax type)
        {
            throw new NotImplementedException();
        }

		public static string TryConvertType(SyntaxNode node)
		{
			if (node == null)
				return null;

			var attrs = Utility.GetCS2HXAttribute(node);
			if (attrs.ContainsKey("ReplaceWithType"))
				return attrs["ReplaceWithType"];

			var typeInfo = TypeState.Instance.GetModel(node).GetTypeInfo(node).ConvertedType;

			if (typeInfo == null || typeInfo is ErrorTypeSymbol)
				return null;

			return ConvertType((TypeSymbol)typeInfo);
		}

        
        public static string ConvertType(SyntaxNode node)
        {
			var ret = TryConvertType(node);

			if (ret == null)
				throw new Exception("Type could not be determined for " + node);

			return ret;
		}

		public static string ConvertType(TypeSymbol typeInfo, bool ignoreGenericArguments = false)
		{
			var array = typeInfo as ArrayTypeSymbol;

			if (array != null)
			{
				if (array.ElementType.ToString() == "byte") //TODO
					return "Bytes"; //byte array becomes the Bytes type
				else
					return "Array<" + ConvertType(array.ElementType) + ">";
			}

			var typeInfoStr = typeInfo.ToString();

			var named = typeInfo as NamedTypeSymbol;

			if (typeInfo.TypeKind == TypeKind.Delegate)
			{
				var dlg = named.DelegateInvokeMethod.As<MethodSymbol>();
				if (dlg.Parameters.Count == 0)
					return "(Void -> " + ConvertType(dlg.ReturnType) + ")";
				else
					return "(" + string.Join("", dlg.Parameters.ToList().Select(o => ConvertType(o.Type) + " -> ")) + ConvertType(dlg.ReturnType) + ")";
			}

			if (!ignoreGenericArguments && named != null && named.TypeArguments.Count > 0)
			{
				return ConvertType(named, true) + "<" + string.Join(", ", named.TypeArguments.ToList().Select(o => ConvertType(o))) + ">";
			}


			switch (typeInfo.Name)
			{
				case "void":
					return "Void";

				case "bool":
				case "Boolean":
					return "Bool";

				case "object":
				case "Object":
					return "Dynamic";

				case "Int64":
				case "UInt64":
				case "Single":
				case "Double":
				case "float":
				case "double":
					return "Float";

				case "String":
				case "string":
					return "String";

				case "int":
				case "Int32":
				case "Byte":
				case "Int16":
				case "UInt16":
				case "Char":
					return "Int";

				default:

					
					//This type does not get translated and gets used as-is
					return typeInfo.Name;
			}

        }

		//public static string ConvertRawType(TypeSyntax type, bool ignoreGenericArguments = false)
		//{
            //Check for the Cs2Hx attribute which could have a directive that tells us what type to use
            

            //var translation = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Type, type.Type, type) as Translations.Type;

            //string genericSuffix;
            //if (type.GenericTypes.Count > 0 && !ignoreGenericArguments)
            //    genericSuffix = "<" + string.Join(", ", type.GenericTypes.Select(o => ConvertRawType(o)).ToArray()) + ">";
            //else
            //    genericSuffix = string.Empty;

            //if (translation != null)
            //    return translation.ReplaceWith + genericSuffix;

            //if (Delegates.ContainsKey(type.Type))
            //{
            //    var dlgs = Delegates[type.Type];
            //    DelegateDeclaration dlg;

            //    if (dlgs.Count() == 1)
            //        dlg = dlgs.Single();
            //    else
            //    {
            //        dlg = dlgs.FirstOrDefault(o => o.Templates.Count == type.GenericTypes.Count);
            //        if (dlg == null)
            //            throw new Exception("Delegate type could not be uniquely identified: " + type.ToString());
            //    }

            //    Func<TypeReference, string> convertDelegateParameter = t =>
            //    {
            //        var template = dlg.Templates.SingleOrDefault(o => o.Name == t.Type);

            //        if (template == null)
            //            return ConvertRawType(t);
            //        else
            //        {
            //            int templatePosition = dlg.Templates.IndexOf(template);
            //            return ConvertRawType(type.GenericTypes[templatePosition]);
            //        }
            //    };

            //    if (dlg.Parameters.Count == 0)
            //        return "(Void -> " + convertDelegateParameter(dlg.ReturnType) + ")";
            //    else
            //        return "(" + string.Join(" -> ", dlg.Parameters.Select(o => convertDelegateParameter(o.TypeReference)).Concat(convertDelegateParameter(dlg.ReturnType)).ToArray()) + ")";
            //}

            //if (type.Type == "Action")
            //{
            //    if (type.GenericTypes.Count == 0)
            //        return "(Void -> Void)";
            //    else
            //        return "(" + string.Join(" -> ", type.GenericTypes.Select(ConvertRawType).ToArray()) + " -> Void)";
            //}

            //if (type.Type == "Func")
            //{
            //    if (type.GenericTypes.Count == 1)
            //        return "(Void -> " + ConvertRawType(type.GenericTypes.Single()) + ")";
            //    else
            //        return "(" + string.Join(" -> ", type.GenericTypes.Select(ConvertRawType).ToArray()) + ")";
            //}

            ////Handle generic types that we can convert
            //switch (type.Type)
            //{
            //    case "IEnumerable":
            //        //return "Iterable" + genericSuffix; I'd love to use Iterable here, but ActionScript can't enumerate on an iterable when using haxe with -as3.    Array also gives a lot more perf since Iterable gets converted to * when using -as3.
            //        return "Array" + genericSuffix;
            //    case "LinkedList":
            //        return "List" + genericSuffix;
            //    case "Queue":
            //    case "List":
            //    case "IList":
            //    case "Stack":
            //        return "Array" + genericSuffix;
            //    case "HashSet":
            //        return "HashSet" + genericSuffix;
            //    case "Dictionary":
            //        return "CSDictionary" + genericSuffix;
            //    case "KeyValuePair":
            //        return "KeyValuePair" + genericSuffix;
            //    case "System.Nullable":
            //    case "Nullable":
            //        return "Nullable_" + ConvertRawType(type.GenericTypes.Single());
            //}

            ////All enums are represented as ints.
            //if (EnumNames.Contains(type.Type))
            //    return "Int";

            //Handle non-generic types
            
        //}

        public static string HaxeLiteral(this LiteralExpressionSyntax literal)
        {
			return literal.ToString();
        }

	}
}
