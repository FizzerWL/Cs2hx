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
                    list.Insert(arg.Location, new IdentifierExpression(arg.Action.Substring(7)));
                else if (arg.Action.StartsWith("InsertVar"))
                {
                    var var = arg.Action.Substring("InsertVar".Length + 1);
                    string result;

                    switch (var)
                    {
                        case "GenericType":
                            result = p.ConvertRawType(invoke.TargetObject.As<MemberReferenceExpression>().TypeArguments.Single());
                            break;
                        default:
                            throw new Exception("Need handler for " + var);
                    }

                    list.Insert(arg.Location, new IdentifierExpression(result));
                }
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
