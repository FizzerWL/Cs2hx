using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class TypeProcessor
    {
		public static string DefaultValue(TypeSyntax type)
		{
			var t = Program.GetModel(type).GetTypeInfo(type).Type;
			if (t.IsValueType == false || t.Name == "Nullable")
				return "null";
			else if (t.SpecialType == SpecialType.System_Boolean)
				return "false";
			else
				return "0";
		}

		public static string TryConvertType(SyntaxNode node)
		{
			if (node == null)
				return null;

			var attrs = Utility.GetCS2HXAttribute(node);
			if (attrs.ContainsKey("ReplaceWithType"))
				return attrs["ReplaceWithType"];

			var typeInfo = Program.GetModel(node).As<ISemanticModel>().GetTypeInfo(node).ConvertedType;

			if (typeInfo == null || typeInfo is ErrorTypeSymbol)
			{
				if (node.ToString() == "byte[]")
					return "Bytes"; //not sure why Roslyn isn't converting these properly, perhaps it's a bug. Just hard-code byte arrays here instead of failing
				else
					return null;
			}

			return ConvertType((TypeSymbol)typeInfo);
		}

		public static string ConvertTypeWithColon(SyntaxNode node)
		{
			var ret = TryConvertType(node);

			if (ret == null)
				return "";
			else
				return ":" + ret;
		}

        
        public static string ConvertType(SyntaxNode node)
        {
			var ret = TryConvertType(node);

			if (ret == null)
				throw new Exception("Type could not be determined for " + node);

			return ret;
		}

		public static string ConvertTypeWithColon(TypeSymbol node)
		{
			var ret = ConvertType(node);

			if (ret == null)
				return "";
			else
				return ":" + ret;
		}

		private static ConcurrentDictionary<TypeSymbol, string> _cachedTypes = new ConcurrentDictionary<TypeSymbol, string>();

		public static string ConvertType(TypeSymbol typeInfo)
		{
			string cachedValue;
			if (_cachedTypes.TryGetValue(typeInfo, out cachedValue))
				return cachedValue;

			cachedValue = ConvertTypeUncached(typeInfo);
			_cachedTypes.TryAdd(typeInfo, cachedValue);
			return cachedValue;
		}

		private static string ConvertTypeUncached(TypeSymbol typeInfo)
		{
			if (typeInfo.IsAnonymousType)
				return null; //leave these blank so haxe can infer

			var array = typeInfo as ArrayTypeSymbol;

			if (array != null)
			{
				if (array.ElementType.ToString() == "byte")
					return "Bytes"; //byte arrays become haxe.io.Bytes
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

			if (typeInfo.TypeKind == TypeKind.Enum)
				return "Int"; //enums are always ints

			if (named != null && named.Name == "Nullable" && named.ContainingNamespace.ToString() == "System")
			{
				//Nullable types get replaced by our Nullable_ alternatives
				return "Nullable_" + ConvertType(named.TypeArguments.Single());
			}

			if (named != null && named.IsGenericType && !named.IsUnboundGenericType)
				return ConvertType(named.ConstructUnboundGenericType()) + "<" + string.Join(", ", TypeArguments(named).Select(o => ConvertType(o))) + ">";

			var typeStr = GenericTypeName(typeInfo);

			switch (typeStr)
			{
				case "System.Void":
					return "Void";

				case "System.Boolean":
					return "Bool";

				case "System.Object":
					return "Dynamic";

				case "System.Int64":
				case "System.UInt64":
				case "System.Single":
				case "System.Double":
					return "Float";

				case "System.String":
					return "String";

				case "System.Int32":
				case "System.UInt32":
				case "System.Byte":
				case "System.Int16":
				case "System.UInt16":
				case "System.Char":
					return "Int";

					
				case "System.Collections.Generic.Dictionary<,>":
					return "CSDictionary"; //change the name to avoid conflicting with Haxe's dictionary type
				case "System.Collections.Generic.List<>":
				case "System.Collections.Generic.IList<>":
				case "System.Collections.Generic.Queue<>":
				case "System.Collections.Generic.Stack<>":
				case "System.Collections.Generic.IEnumerable<>":
				case "System.Collections.Generic.Dictionary<,>.ValueCollection":
				case "System.Collections.Generic.Dictionary<,>.KeyCollection":
				case "System.Linq.IOrderedEnumerable<>":
					return "Array";

				case "System.Array":
					return null; //in haxe, unlike C#, array must always have type arguments.  To work around this, just avoid printing the type anytime we see a bare Array type in C#. haxe will infer it.

				case "System.Collections.Generic.LinkedList<>":
					return "List";

				default:
					var trans = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Type, typeStr, null);

					if (trans != null)
						return trans.As<Translations.Type>().ReplaceWith;

					if (named != null)
						return WriteType.TypeName(named);

					//This type does not get translated and gets used as-is
					return typeInfo.Name;
				
			}

        }

		private static IEnumerable<TypeSymbol> TypeArguments(NamedTypeSymbol named)
		{
			if (named.ContainingType != null)
			{
				//Hard-code generic types for subclasses, since I can't find a way to determine them programatically
				switch (named.Name)
				{
					case "ValueCollection":
						return new[] { named.ContainingType.TypeArguments.ElementAt(1) };
					case "KeyCollection":
						return new[] { named.ContainingType.TypeArguments.ElementAt(0) };
					default:
						throw new Exception("Handler for generic subclass " + named);
				}
			}

			return named.TypeArguments.ToList();
		}


        public static string HaxeLiteral(this SyntaxNode literal)
        {
			if (literal is ArgumentSyntax)
				return HaxeLiteral(literal.As<ArgumentSyntax>().Expression);
			else if (literal is LiteralExpressionSyntax)
				return literal.ToString();
			else
				throw new Exception("Need handler for " + literal.GetType().Name);
        }


		public static string GenericTypeName(TypeSymbol typeSymbol)
		{
			if (typeSymbol == null)
				return null;

			var named = typeSymbol as NamedTypeSymbol;
			var array = typeSymbol as ArrayTypeSymbol;

			if (array != null)
				return GenericTypeName(array.ElementType) + "[]";
			else if (named != null && named.IsGenericType && !named.IsUnboundGenericType)
				return GenericTypeName(named.ConstructUnboundGenericType());
			else if (named != null && named.SpecialType != SpecialType.None)
				return named.ContainingNamespace + "." + named.Name; //this forces C# shortcuts like "int" to never be used, and instead returns System.Int32 etc.
			else
				return typeSymbol.ToString();
		}

		/// <summary>
		/// Convert a type string into a string for matching Translations.xml.  We exclude generic suffixes just because xml requires encoding < and >
		/// </summary>
		public static string MatchString(string typeStr)
		{
			if (typeStr == null)
				return null;
			else if (typeStr.EndsWith("[]"))
				return "System.Array";
			else
				return Regex.Replace(typeStr, @"[<>,]", "");
		}

		public static string RemoveGenericArguments(string haxeType)
		{
			var i = haxeType.IndexOf('<');
			if (i == -1)
				return haxeType;
			else
				return haxeType.Substring(0, i);
		}

		/// <summary>
		/// Returns true if this is a value type in C# but a reference type in haxe.  We try to avoid this where possible, but haxe doesn't let us define our own value types.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool ValueToReference(TypeSyntax type)
		{
			var typeSymbol = Program.GetModel(type).GetTypeInfo(type);
			if (typeSymbol.Type.IsValueType == false)
				return false;

			switch (ConvertType(typeSymbol.Type))
			{
				case "Int":
				case "Float":
				case "Bool":
				case "String":
					return false;
				default:
					return true;
			}
		}
	}
}
