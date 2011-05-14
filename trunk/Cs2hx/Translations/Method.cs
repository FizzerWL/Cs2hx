using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.NRefactory.Ast;
using System.Diagnostics;

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

        private string ReplaceSpecialIndicators(string rawString, Program p, InvocationExpression invoke)
        {
            if (rawString.Contains("{genericType}"))
                rawString = ReplaceGenericVar(rawString, p, invoke);

            if (rawString.Contains("{varName}"))
                rawString = ReplaceVarName(rawString, p, invoke);

            return rawString;
        }

        private string ReplaceVarName(string rawString, Program p, InvocationExpression invoke)
        {
            var memberReference = invoke.TargetObject.As<MemberReferenceExpression>();

            var identifier = memberReference.TargetObject as IdentifierExpression;

            if (identifier == null)
                throw new Exception("Calling method " + memberReference.MemberName + " needs to be called directly from an identifier, since we need to reference this object in its arguments list.");

            var varName = identifier.Identifier;

            return rawString.Replace("{varName}", varName);
        }

        private string ReplaceGenericVar(string rawString, Program p, InvocationExpression invoke)
        {
            var genericVar = p.ConvertRawType(invoke.TargetObject.As<MemberReferenceExpression>().TypeArguments.Single());

            return rawString.Replace("{genericType}", genericVar);
        }


        internal IEnumerable<Expression> TranslateParameters(List<Expression> list, InvocationExpression invoke, Program p)
        {
            //Copy it
            list = list.ToList();

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
                    list.Insert(arg.Location, new IdentifierExpression(ReplaceSpecialIndicators(arg.Action.Substring(7), p, invoke)));
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
