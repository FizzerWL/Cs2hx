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

		public static Translation GetTranslation(TranslationType type, string objectName, TypeSyntax typeReference)
		{
			string sourceTypeName;

			if (typeReference == null)
				sourceTypeName = null;
			else
				sourceTypeName = typeReference.ToString();

			return GetTranslation(type, objectName, sourceTypeName);
		}

		public static Translation GetTranslation(TranslationType type, string objectName, ExpressionSyntax expression)
		{
			string source = null;

			//if (expression is IdentifierNameSyntax)
			//{
			//	var identifier = expression.As<IdentifierNameSyntax>();

			//	//TypeSyntax typeRef;
			//	//if (Utility.TryFindType(identifier, out typeRef))
			//	//	source = typeRef.Type;
			//	//else
			//		source = identifier.Identifier.ToString();
			//}
			//else if (expression is MemberAccessExpressionSyntax && expression.As<MemberAccessExpressionSyntax>().Name is ThisExpressionSyntax)
			//{
			//	//If an expression is simply this.<expr>, we can still easily identify it's type just as we could if it was an identifier
			//	var identifier = expression.As<MemberReferenceExpression>().MemberName;
			//	TypeReference typeRef;
			//	if (Utility.TryFindType(identifier, expression, out typeRef))
			//		source = typeRef.Type;
			//	else
			//		source = identifier;
			//}

			return GetTranslation(type, objectName, source);
		}

		public static Translation GetTranslation(TranslationType type, string objectName, string sourceTypeName)
		{
			if (string.IsNullOrEmpty(sourceTypeName))
				sourceTypeName = "*";

			var matches = Program.TranslationDocs.SelectMany(o => o.XPathSelectElements("/Translations/" + type.ToString() + "[(@SourceObject = '*' or @SourceObject = '" + sourceTypeName + "') and @Match='" + objectName + "']")).ToList();

			if (matches.Count == 0)
				return null;

			if (matches.Count > 1)
			{
				//Try to resolve duplicates.  If one is an exact match and others are a wildcard match, we can safely assume we want the exact match.
				var exactMatches = matches.Except(matches.Where(o => o.Attribute("SourceObject").Value == "*")).ToList();

				if (exactMatches.Count == 1)
					matches = exactMatches;
				else
					throw new Exception("Multiple matches for " + objectName);
			}

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
