using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cs2hx.Translations
{
    class Type : Translation
    {
        public string ReplaceWith { get; set; }
		public bool SkipGenericTypes { get; set; }

		internal string Replace(Roslyn.Compilers.CSharp.NamedTypeSymbol typeInfo)
		{
			if (ReplaceWith.StartsWith("{"))
			{
				var args = ReplaceWith.Substring(1, ReplaceWith.Length - 2).Split(' ');
				switch (args[0])
				{
					case "typeparameter":
						return TypeProcessor.ConvertType(typeInfo.TypeArguments.ElementAt(int.Parse(args[1])));

					default:
						throw new Exception("Invalid parameter: " + args[0]);
				}
			}
			else
				return this.ReplaceWith;
		}
	}
}
