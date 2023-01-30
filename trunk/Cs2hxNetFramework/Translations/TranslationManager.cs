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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;


namespace Cs2hx.Translations
{
    class TranslationManager
    {
		internal static List<MethodTranslation> Methods;
		internal static List<PropertyTranslation> Properties;
		internal static List<TypeTranslation> Types;
        internal static List<string> References;
		
		public static void Init(IEnumerable<string> extraDocs)
		{
			Methods = new List<MethodTranslation>();
			Properties = new List<PropertyTranslation>();
			Types = new List<TypeTranslation>();
            References = new List<string>();

			foreach(var element in BuildTranslationDocs(extraDocs).SelectMany(o => o.Root.Elements()))
			{
				switch (element.Name.LocalName)
				{
					case "Method":
						Methods.Add(new MethodTranslation(element));
						break;
					case "Type":
						Types.Add(new TypeTranslation(element));
						break;
					case "Property":
						Properties.Add(new PropertyTranslation(element));
						break;
                    case "Reference":
                        References.Add(element.Value);
                        break;
					default:
						throw new Exception("Unexpected type name " + element.Name);
				}
			}
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


        public static T InitProperties<T>(T obj, XElement data)
        {
            foreach (var prop in obj.GetType().GetProperties().Where(o => o.CanWrite))
            {
                var attr = data.Attribute(prop.Name);
                if (attr != null)
                {
					if (prop.PropertyType == typeof(string))
						prop.SetValue(obj, attr.Value);
					else if (prop.PropertyType == typeof(bool))
						prop.SetValue(obj, bool.Parse(attr.Value));
					else if (prop.PropertyType == typeof(int))
						prop.SetValue(obj, int.Parse(attr.Value));
					else
						throw new Exception("Need handler for " + prop.PropertyType);
                }
            }

			return obj;
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


	}
}
