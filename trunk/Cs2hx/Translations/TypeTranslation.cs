using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Cs2hx.Translations
{
    class TypeTranslation
    {
		public TypeTranslation(XElement data)
		{
			TranslationManager.InitProperties(this, data);
		}


		public string Match { get; set; }
        public string ReplaceWith { get; set; }
		public bool SkipGenericTypes { get; set; }

		internal string Replace(Microsoft.CodeAnalysis.INamedTypeSymbol typeInfo)
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


		public static TypeTranslation Get(string typeStr)
		{
			var match = TranslationManager.MatchString(typeStr);

			var matches = TranslationManager.Types.Where(o => o.Match == match).ToList();

			if (matches.Count > 1)
				throw new Exception("Multiple matches for " + match);

			return matches.SingleOrDefault();
		}

	}
}
