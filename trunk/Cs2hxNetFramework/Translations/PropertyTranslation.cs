using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Cs2hx.Translations
{
    class PropertyTranslation
    {
		public PropertyTranslation(XElement data)
		{
			TranslationManager.InitProperties(this, data);
		}

		public string SourceObject { get; set; }
		public string Match { get; set; }
        public string ReplaceWith { get; set; }
        public string ExtensionNamespace { get; set; }
        public bool SkipExtensionParameter { get; set; }


        public static PropertyTranslation Get(string typeStr, string memberName)
		{
			var sourceObj = TranslationManager.MatchString(typeStr);


			var matches = TranslationManager.Properties.Where(o => o.Match == memberName && o.SourceObject == sourceObj).ToList();

			if (matches.Count > 1)
				throw new Exception("Multiple matches for " + sourceObj + " " + memberName);

			return matches.SingleOrDefault();
		}
    }
}
