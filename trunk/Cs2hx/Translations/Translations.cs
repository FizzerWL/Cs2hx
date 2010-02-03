using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Ast;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Configuration;

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
            foreach (var path in extra.Concat(builtInPath))
                ret.Add(XDocument.Load(path));

            return ret;
        }

        public static Translation GetTranslation(IEnumerable<XDocument> docs, TranslationType type, string objectName, TypeReference typeReference)
        {
            string sourceTypeName;

            if (typeReference == null || typeReference.IsNull)
                sourceTypeName = null;
            else
                sourceTypeName = typeReference.Type;

            return GetTranslation(docs, type, objectName, sourceTypeName);
        }

        public static Translation GetTranslation(IEnumerable<XDocument> docs, TranslationType type, string objectName, Expression expression)
        {
            string source = null;

            if (expression is IdentifierExpression)
            {
                var identifier = expression.As<IdentifierExpression>();

                TypeReference typeRef;
                if (Utility.TryFindType(identifier, out typeRef))
                    source = typeRef.Type;
                else
                    source = identifier.Identifier;
            }
            else if (expression is TypeReferenceExpression)
                source = expression.As<TypeReferenceExpression>().TypeReference.Type;

            return GetTranslation(docs, type, objectName, source);
        }

        public static IEnumerable<string> ExtraImports(IEnumerable<XDocument> docs)
        {
            return docs.SelectMany(o => o.XPathSelectElements("/Translations/ExtraImport")).Select(o => o.Attribute("Import").Value);
        }

        public static Translation GetTranslation(IEnumerable<XDocument> docs, TranslationType type, string objectName, string sourceTypeName)
        {
            if (string.IsNullOrEmpty(sourceTypeName))
                sourceTypeName = "*";

            var matches = docs.SelectMany(o => o.XPathSelectElements("/Translations/" + type.ToString() + "[(@SourceObject = '*' or @SourceObject = '" + sourceTypeName + "') and @Match='" + objectName + "']")).ToList();

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
