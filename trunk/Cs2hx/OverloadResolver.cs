using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	public static class OverloadResolver
	{
		static ConcurrentDictionary<MethodSymbol, string> _methodNameCache = new ConcurrentDictionary<MethodSymbol, string>();

		public static string MethodName(MethodSymbol method)
		{
			string ret;
			if (_methodNameCache.TryGetValue(method, out ret))
				return ret;

			ret = MethodNameUncached(method);

			_methodNameCache.TryAdd(method, ret);
			return ret;
		}

		private static string MethodNameUncached(MethodSymbol method)
		{
			var overloadedGroup = method.ContainingType.GetMembers(method.Name).OfType<MethodSymbol>().ToList();

			if (overloadedGroup.Count == 0)
				throw new Exception("Symbols not found");

			if (overloadedGroup.Count == 1)
				return method.Name;

			var defaultOverloadOpt = PickDefault(overloadedGroup);
			
			if (method == defaultOverloadOpt || method.ConstructedFrom == defaultOverloadOpt)
				return method.Name; //return the name unchanged

			return ExpandedMethodName(method);
		}

		private static string ExpandedMethodName(MethodSymbol method)
		{
			
			var ret = new StringBuilder(20);

			ret.Append(method.Name);
			ret.Append("_");

			foreach (var param in method.Parameters)
			{
				ret.Append(param.Type.Name);

				var named = param.Type as NamedTypeSymbol;
				if (named != null)
					foreach(var typeArg in named.TypeArguments)
						if (typeArg.TypeKind != TypeKind.TypeParameter)
							ret.Append(typeArg.Name);

				ret.Append("_");
			}

			ret.Remove(ret.Length - 1, 1);
			return ret.ToString();
		}

		private static MethodSymbol PickDefault(List<MethodSymbol> overloadedGroup)
		{
			var first = overloadedGroup.First();

			//Hard-code the default overload for many common functions that we use. This isn't necessary to run correctly, but it produces nicer code.
			if (first.Name == "WriteLine" && first.ContainingType.Name == "Console")
				return overloadedGroup.Single(o => o.Parameters.Count == 1 && o.Parameters.Single().Type.Name == "String");
			if (first.Name == "ToDictionary" && first.ContainingType.Name == "Enumerable")
				return overloadedGroup[2];
			if (first.Name == "Append" && first.ContainingType.Name == "StringBuilder")
				return overloadedGroup[2];
			if (first.Name == "AppendLine" && first.ContainingType.Name == "StringBuilder")
				return overloadedGroup[1];

			//By default, use the overload that takes the fewest parameters as the default
			int minParams = overloadedGroup.Min(o => o.Parameters.Count);

			var overloadsWithMinParams = overloadedGroup.Where(o => o.Parameters.Count == minParams).ToList();

			if (overloadsWithMinParams.Count == 1)
				return overloadsWithMinParams[0];

			//If multiple overloads take the fewest number of parameters, just pick one arbitrarily
			return overloadedGroup[0];
		}
	}
}
