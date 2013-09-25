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
        }

		private string ReplaceSpecialIndicators(string rawString, InvocationExpressionSyntax invoke)
		{
			if (rawString.Contains("{genericType}"))
				rawString = ReplaceGenericVar(rawString, invoke);

			if (rawString.Contains("{varName}"))
				rawString = ReplaceVarName(rawString, invoke);

			return rawString;
		}

		private string ReplaceVarName(string rawString, InvocationExpressionSyntax invoke)
		{
			var memberReference = invoke.Expression.As<MemberAccessExpressionSyntax>();

			var identifier = memberReference.Expression as IdentifierNameSyntax;

			if (identifier == null)
				throw new Exception("Calling method " + memberReference.Name + " needs to be called directly from an identifier, since we need to reference this object in its arguments list.");

			var varName = identifier.Identifier.ValueText;

			return rawString.Replace("{varName}", varName);
		}

		private string ReplaceGenericVar(string rawString, InvocationExpressionSyntax invoke)
		{
			var name = invoke.Expression.As<MemberAccessExpressionSyntax>().Name.As<GenericNameSyntax>();

			var genericVar = TypeProcessor.ConvertType(name.TypeArgumentList.Arguments.Single());

			return rawString.Replace("{genericType}", genericVar);
		}


		internal IEnumerable<ExpressionSyntax> TranslateParameters(IEnumerable<ExpressionSyntax> args, InvocationExpressionSyntax invoke)
		{
			//Copy it
			var list = args.ToList();

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
					list.Insert(arg.Location, SyntaxTree.ParseText(ReplaceSpecialIndicators(arg.Action.Substring(7), invoke)).GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Single());
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
