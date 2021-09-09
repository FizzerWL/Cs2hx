using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cs2hx
{
    public static class Utility
    {

		public static void Parallel<T>(this IEnumerable<T> list, Action<T> action, bool parallel = true)
		{
            if (parallel)
                System.Threading.Tasks.Parallel.ForEach(list, action);
            else
            {
                foreach (var t in list)
                    action(t);
            }
		}

        public static int ValueOrZero<T>(this Dictionary<T, int> a, T key)
        {
            int i;
            if (a.TryGetValue(key, out i))
                return i;
            else
                return 0;
        }

        public static void AddTo<T>(this Dictionary<T, int> a, T key, int sumToAdd)
        {
            if (a == null)
                throw new Exception("AddTo passed null dictionary");
            if (a.ContainsKey(key))
                a[key] += sumToAdd;
            else
                a.Add(key, sumToAdd);
        }

        public static string FullName(this INamespaceSymbol ns)
		{
			if (ns.IsGlobalNamespace)
				return "";
			else
				return ns.ToString();
		}
		public static string FullNameWithDot(this INamespaceSymbol ns)
		{
			if (ns.IsGlobalNamespace)
				return "";
			else
				return ns.ToString() + ".";
		}

		public static string AttributeOrNull(this XElement element, string attrName)
		{
			var a = element.Attribute(attrName);
			if (a == null)
				return null;
			else
				return a.Value;
		}

        public static T As<T>(this object o)
        {
            return (T)o;
        }

		public static string SubstringSafe(this string s, int startAt, int length)
		{
			if (s.Length < startAt + length)
				return s.Substring(startAt);
			else
				return s.Substring(startAt, length);
		}

		public static bool None<T>(this IEnumerable<T> a, Func<T, bool> pred)
		{
			return !a.Any(pred);
		}

		public static bool None<T>(this IEnumerable<T> a)
		{
			return !a.Any();
		}

		public static string SubstringAfterLast(this string s, char c)
		{
			int i = s.LastIndexOf(c);
			if (i == -1)
				throw new Exception("char not found");
			return s.Substring(i + 1);
		}
		public static string TrySubstringAfterLast(this string s, char c)
		{
			int i = s.LastIndexOf(c);
			if (i == -1)
				return s;
			else 
				return s.Substring(i + 1);
		}
		public static string TrySubstringBeforeFirst(this string s, char c)
		{
			int i = s.IndexOf(c);
			if (i == -1)
				return s;
			else
				return s.Substring(0, i);
		}

		public static string SubstringBeforeLast(this string s, char c)
		{
			int i = s.LastIndexOf(c);
			if (i == -1)
				throw new Exception("char not found");
			return s.Substring(0, i);
		}

		public static IMethodSymbol UnReduce(this IMethodSymbol methodSymbol)
		{
            if (methodSymbol == null)
                throw new Exception("methodSymbol is null");
			while (methodSymbol.ReducedFrom != null)
				methodSymbol = methodSymbol.ReducedFrom;

			return methodSymbol;
		}

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> array, bool throwOnDuplicate)
        {
            var hs = new HashSet<T>();
            foreach (var t in array)
            {
                if (throwOnDuplicate && hs.Contains(t))
                    throw new ArgumentException("Duplicate key: " + t.ToString());
                hs.Add(t);
            }
            return hs;
        }


        public static IEnumerable<T> Concat<T>(this IEnumerable<T> array, T item)
        {
            return array.Concat(new T[] { item });
        }
        public static IEnumerable<T> Except<T>(this IEnumerable<T> array, T item)
        {
            return array.Except(new T[] { item });
        }



        public static string Descriptor(SyntaxNode node)
        {
            var sb = new StringBuilder();
			sb.Append(node.Span.ToString() + " ");

            while (node != null)
            {
				

                if (node is BaseTypeDeclarationSyntax)
                    sb.Append("Type: " + node.As<BaseTypeDeclarationSyntax>().Identifier.ValueText + ", ");
                else if (node is MethodDeclarationSyntax)
                    sb.Append("Method: " + node.As<MethodDeclarationSyntax>().Identifier.ValueText + ", ");
                else if (node is PropertyDeclarationSyntax)
                    sb.Append("Property: " + node.As<PropertyDeclarationSyntax>().Identifier.ValueText + ", ");
                node = node.Parent;
            }

            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public static string GetAttributeName(AttributeArgumentSyntax attr)
        {
			return attr.NameEquals.Name.ToString();
        }
		public static string RemoveFromStartOfString(this string s, string toRemove)
		{
			if (!s.StartsWith(toRemove))
				throw new Exception("Does not start string: " + s);
			return s.Substring(toRemove.Length);
		}

		public static string RemoveFromEndOfString(this string s, string toRemove)
		{
			if (!s.EndsWith(toRemove))
				throw new Exception("Does not end string: " + s);
			return s.Substring(0, s.Length - toRemove.Length);
		}

		public static string TryGetIdentifier(ExpressionSyntax expression)
		{
			var identifier = expression as IdentifierNameSyntax;

			if (identifier != null)
				return identifier.Identifier.ValueText;

			var thisSyntax = expression as ThisExpressionSyntax;
			if (thisSyntax != null)
				return "this";

			var memAccess = expression as MemberAccessExpressionSyntax;
			if (memAccess != null && memAccess.Expression is ThisExpressionSyntax)
				return memAccess.ToString();

			return null;

		}

        internal static IEnumerable<ITypeSymbol> AllTypes(ITypeSymbol type)
        {
            yield return type;

            if (type is INamedTypeSymbol)
            {
                foreach (var ta in type.As<INamedTypeSymbol>().TypeArguments)
                    foreach (var t in AllTypes(ta))
                        yield return t;
            }
            else if (type is IArrayTypeSymbol)
                yield return type.As<IArrayTypeSymbol>().ElementType;
        }

        public static IEnumerable<ITypeSymbol> PassTypeArgsToMethod(IMethodSymbol methodSymbol)
        {
            
            var types = methodSymbol.TypeParameters.Except(methodSymbol.Parameters.SelectMany(o => Utility.AllTypes(o.Type))).Distinct().ToList();

            foreach (var t in types)
                yield return t;
        }
    }
}
