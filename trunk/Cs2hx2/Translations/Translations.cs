using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using Roslyn.Compilers.CSharp;


namespace Cs2hx.Translations
{
    abstract class Translation
    {
        public enum TranslationType
        {
            Method, Property, Type
        }

        public static List<XDocument> BuildTranslationDocs(IEnumerable<string> extra)
        {
            string builtInPath = ConfigurationManager.AppSettings["PathToTranslationsXml"];

            if (!File.Exists(builtInPath))
                builtInPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Translations.xml");

            var ret = new List<XDocument>();
            foreach (var xml in extra.Concat(File.ReadAllText(builtInPath)))
                ret.Add(XDocument.Parse(xml));

            return ret;
        }

		public static Translation GetTranslation(TranslationType type, string objectName, string sourceTypeName, string arguments = null)
		{
			if (string.IsNullOrEmpty(sourceTypeName))
				sourceTypeName = "*";

			var matches = Program.TranslationDocs.SelectMany(o => o.XPathSelectElements("/Translations/" + type.ToString() + "[(not(@SourceObject) or @SourceObject = '*' or @SourceObject = '" + sourceTypeName + "') and @Match='" + objectName + "']")).ToList();

			if (matches.Count > 1)
			{
				var matches2 = matches.Where(o => o.AttributeOrNull("ArgumentTypes") == arguments).ToList();

				if (matches2.Count > 0)
					matches = matches2;
				else
					matches = matches.Where(o => o.AttributeOrNull("ArgumentTypes") == null).ToList();

				if (matches.Count > 1)
				{
					matches = matches.Except(matches.Where(o => o.Attribute("SourceObject").Value == "*")).ToList();	

				}
			}

			if (matches.Count == 0)
				return null;

			var match = matches.Single();

			var trans = (Translation)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType("Cs2hx.Translations." + match.Name.LocalName));
			trans.Init(match);

			return trans;
		}

		public static string ExtensionName(NamedTypeSymbol symbol)
		{
			var trans = Program.TranslationDocs.SelectMany(o => o.XPathSelectElements("/Translations/ExtensionType[@Match='" + symbol.ContainingNamespace + "." + symbol.Name + "']")).SingleOrDefault();

			if (trans == null)
				return symbol.Name;
			else
				return trans.Attribute("ReplaceWith").Value.SubstringAfterLast('.');
		}



		public static IEnumerable<string> ExtraImports()
		{
			return Program.TranslationDocs.SelectMany(o => o.XPathSelectElements("/Translations/ExtraImport")).Select(o => o.Attribute("Import").Value);
		}


        protected virtual void Init(XElement data)
        {
            foreach (var prop in this.GetType().GetProperties().Where(o => o.CanWrite))
            {
                var attr = data.Attribute(prop.Name);
                if (attr != null)
                {
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(this, attr.Value, null);
                    else if (prop.PropertyType == typeof(bool))
                        prop.SetValue(this, bool.Parse(attr.Value), null);
                    else
                        throw new Exception("Need handler for " + prop.PropertyType);
                }
            }
        }



	}
}
