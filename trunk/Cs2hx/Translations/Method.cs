using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using Roslyn.Compilers.CSharp;

namespace Cs2hx.Translations
{
    class Method : Translation
    {
        public string ReplaceWith { get; set; }
        public List<ArgumentModifier> Arguments { get; set; }
        public string ExtensionNamespace { get; set; }
        public bool SkipExtensionParameter { get; set; }
		public string ArgumentTypes { get; set; }

		public bool HasComplexReplaceWith
		{
			get { return DoComplexReplaceWith != null; }
		}
		public Action<HaxeWriter, MemberAccessExpressionSyntax> DoComplexReplaceWith;

        public bool IsExtensionMethod
        {
            get
            {
                return this.ExtensionNamespace != null;
            }
        }

        protected override void Init(XElement data)
        {
            base.Init(data);

            Arguments = data.Elements("Argument").Select(o =>
                new ArgumentModifier()
                {
                    Action = o.Attribute("Action").Value,
                    Location = int.Parse(o.Attribute("Location").Value)
                }).ToList();

			if (data.Element("ReplaceWith") != null)
			{
				DoComplexReplaceWith = (writer, expression) =>
					{
						foreach (var element in data.Element("ReplaceWith").Elements())
						{
							switch (element.Name.LocalName)
							{
								case "String":
									writer.Write(ReplaceSpecialIndicators(element.Value, expression));
									break;
								case "Expression":
									Core.Write(writer, expression.Expression);
									break;
								default:
									throw new Exception("Unexpected element name " + element.Name);
							}
						}
					};
			}
        }

		private string ReplaceSpecialIndicators(string rawString, ExpressionSyntax expression)
		{
			if (rawString.Contains("{genericType}"))
				rawString = ReplaceGenericVar(rawString, expression);

			if (rawString.Contains("{varName}"))
				rawString = ReplaceVarName(rawString, expression);

			return rawString;
		}

		private string ReplaceVarName(string rawString, ExpressionSyntax expression)
		{
			var memberReference = expression.As<MemberAccessExpressionSyntax>();

			var identifier = memberReference.Expression as IdentifierNameSyntax;

			if (identifier == null)
				throw new Exception("Calling method " + memberReference.Name + " needs to be called directly from an identifier, since we need to reference this object in its arguments list.");

			var varName = identifier.Identifier.ValueText;

			return rawString.Replace("{varName}", varName);
		}

		private string ReplaceGenericVar(string rawString, ExpressionSyntax expression)
		{
			var name = expression.As<MemberAccessExpressionSyntax>().Name.As<GenericNameSyntax>();

			var genericVar = TypeProcessor.ConvertType(name.TypeArgumentList.Arguments.Single());

			return rawString.Replace("{genericType}", genericVar);
		}


		internal IEnumerable<ExpressionOrString> TranslateParameters(IEnumerable<ExpressionSyntax> args, ExpressionSyntax expression)
		{
			//Copy it
			var list = args.Select(o => new ExpressionOrString { Expression = o }).ToList();

			foreach (var arg in Arguments)
			{
				if (arg.Action == "Delete")
					list.RemoveAt(arg.Location);
				else if (arg.Action == "DeleteIfPresent")
				{
					if (list.Count > arg.Location)
						list.RemoveAt(arg.Location);
				}
				else if (arg.Action.StartsWith("MoveTo "))
				{
					var item = list[arg.Location];
					list.RemoveAt(arg.Location);
					list.Insert(int.Parse(arg.Action.Substring(7)), item);
				}
				else if (arg.Action.StartsWith("Insert "))
					list.Insert(arg.Location, new ExpressionOrString { String = ReplaceSpecialIndicators(arg.Action.Substring(7), expression)});
				else
					throw new Exception("Need handler for " + arg.Action);
			}

			return list;
		}
    }

    class ArgumentModifier
    {
        public int Location { get; set; }
        public string Action { get; set; }
    }
}
