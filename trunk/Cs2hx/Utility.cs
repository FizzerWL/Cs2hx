using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Ast;
using System.Diagnostics;

namespace Cs2hx
{
    public static class Utility
    {
        public static T As<T>(this object o)
        {
            return (T)o;
        }

        public static bool Has(this Modifiers mods, Modifiers check)
        {
            return (mods & check) == check;
        }
        public static bool Has(this ParameterModifiers mods, ParameterModifiers check)
        {
            return (mods & check) == check;
        }

        public static string GetMethodName(INode statement)
        {
            return GetMethod(statement).Name;
        }

        public static MethodDeclaration GetMethod(INode statement)
        {
            while (!(statement is MethodDeclaration))
            {
                if (statement.Parent == null)
                    return null;
                statement = statement.Parent;
            }

            return statement.As<MethodDeclaration>();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> array, bool throwOnDuplicate)
        {
            var hs = new HashSet<T>();
            foreach (var t in array)
            {
                if (throwOnDuplicate && hs.Contains(t))
                    throw new ArgumentException("Duplicate key: " + t.ToString());
                hs.Add(t);
            }
            return hs;
        }


        public static IEnumerable<T> Concat<T>(this IEnumerable<T> array, T item)
        {
            return array.Concat(new T[] { item });
        }
        public static IEnumerable<T> Except<T>(this IEnumerable<T> array, T item)
        {
            return array.Except(new T[] { item });
        }



        /// <summary>
        /// Identifies the type of the passed variable declaration
        /// </summary>
        /// <param name="localVariableDeclaration"></param>
        /// <returns></returns>
        public static TypeReference DetermineType(LocalVariableDeclaration localVariableDeclaration)
        {
            if (localVariableDeclaration.TypeReference.Type == "var")
            {
                //If var is used, try to identify the type by the right-hand identifier
                foreach (var inititalizers in localVariableDeclaration.Variables)
                {
                    if (inititalizers.Initializer is ObjectCreateExpression)
                        return inititalizers.Initializer.As<ObjectCreateExpression>().CreateType;
                }
            }

            return localVariableDeclaration.TypeReference;
        }

        /// <summary>
        /// Attempts to determine the type of an identity by looking up the call tree to find a local variable with its name
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryFindType(IdentifierExpression identifier, out TypeReference type)
        {
            return TryFindType(identifier, identifier, out type);
        }

        private static bool TryFindType(IdentifierExpression identifier, INode startAt, out TypeReference type)
        {
            if (startAt == null)
            {
                type = null;
                return false;
            }

            if (startAt is MethodDeclaration)
            {
                //Check to see if it's a parameter
                var method = startAt.As<MethodDeclaration>();
                var prm = method.Parameters.SingleOrDefault(o => o.ParameterName == identifier.Identifier);

                if (prm != null)
                {
                    type = prm.TypeReference;
                    return true;
                }
            }
            else if (startAt is ForeachStatement)
            {
                //Check to see if it's an iterator variable
                var forEach = startAt.As<ForeachStatement>();
                if (forEach.VariableName == identifier.Identifier)
                {
                    type = forEach.TypeReference;
                    return true;
                }
            }

            //Walk up and back
            for (int i = startAt.Parent.Children.IndexOf(startAt) - 1; i >= 0; i--)
            {
                var t = startAt.Parent.Children[i];
                if (t is LocalVariableDeclaration)
                {
                    var lvd = t.As<LocalVariableDeclaration>();
                    foreach (var declaration in lvd.Variables)
                    {
                        if (declaration.Name == identifier.Identifier)
                        {
                            type = DetermineType(lvd);
                            return true;
                        }
                    }
                }
                else if (t is FieldDeclaration)
                {
                    var fields = t.As<FieldDeclaration>();

                    if (fields.Fields.Any(o => o.Name == identifier.Identifier))
                    {
                        type = fields.TypeReference;
                        return true;
                    }
                }
            }

            if (startAt.Parent.Parent != null && TryFindType(identifier, startAt.Parent, out type))
                return true;

            type = null;
            return false;
        }



        public static string Descriptor(INode node)
        {
            StringBuilder sb = new StringBuilder();

            while (node != null)
            {
                if (node is TypeDeclaration)
                    sb.Append("Type: " + node.As<TypeDeclaration>().Name + ", ");
                else if (node is MethodDeclaration)
                    sb.Append("Method: " + node.As<MethodDeclaration>().Name + ", ");
                else if (node is PropertyDeclaration)
                    sb.Append("Property: " + node.As<PropertyDeclaration>().Name + ", ");
                node = node.Parent;
            }

            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public static IEnumerable<INode> RecurseAllChildren(INode node)
        {
            var ret = new List<INode>();
            ret.Add(node);
            foreach (var c in LogicalChildren(node))
                ret.AddRange(RecurseAllChildren(c));
            return ret;
        }

        public static IEnumerable<INode> LogicalChildren(this INode node)
        {
            var ret = node.Children.ToList();

            if (node is StatementWithEmbeddedStatement)
                ret.Add(node.As<StatementWithEmbeddedStatement>().EmbeddedStatement);



            if (node is ExpressionStatement)
                ret.Add(node.As<ExpressionStatement>().Expression);
            else if (node is CastExpression)
                ret.Add(node.As<CastExpression>().Expression);
            else if (node is ParenthesizedExpression)
                ret.Add(node.As<ParenthesizedExpression>().Expression);
            else if (node is ElseIfSection)
                ret.Add(node.As<ElseIfSection>().Condition);
            else if (node is IndexerExpression)
            {
                ret.Add(node.As<IndexerExpression>().TargetObject);
                ret.AddRange(node.As<IndexerExpression>().Indexes.Cast<INode>());
            }
            else if (node is ArrayCreateExpression)
            {
                ret.AddRange(node.As<ArrayCreateExpression>().Arguments.Cast<INode>());
                ret.Add(node.As<ArrayCreateExpression>().ArrayInitializer);
            }
            else if (node is LambdaExpression)
            {
                ret.Add(node.As<LambdaExpression>().ExpressionBody);
                ret.Add(node.As<LambdaExpression>().StatementBody);
            }
            else if (node is UnaryOperatorExpression)
                ret.Add(node.As<UnaryOperatorExpression>().Expression);
            else if (node is PropertyDeclaration)
            {
                ret.Add(node.As<PropertyDeclaration>().GetRegion);
                ret.Add(node.As<PropertyDeclaration>().SetRegion);
            }
            else if (node is ConditionalExpression)
            {
                ret.Add(node.As<ConditionalExpression>().TrueExpression);
                ret.Add(node.As<ConditionalExpression>().FalseExpression);
            }
            else if (node is ForStatement)
            {
                ret.Add(node.As<ForStatement>().Condition);
                ret.AddRange(node.As<ForStatement>().Initializers.Cast<INode>());
                ret.AddRange(node.As<ForStatement>().Iterator.Cast<INode>());
            }
            else if (node is FieldDeclaration)
                ret.AddRange(node.As<FieldDeclaration>().Fields.Cast<INode>());
            else if (node is VariableDeclaration)
            {
                ret.Add(node.As<VariableDeclaration>().FixedArrayInitialization);
                ret.Add(node.As<VariableDeclaration>().Initializer);
            }
            else if (node is MethodDeclaration)
                ret.Add(node.As<MethodDeclaration>().Body);
            else if (node is InvocationExpression)
            {
                ret.Add(node.As<InvocationExpression>().TargetObject);
                ret.AddRange(node.As<InvocationExpression>().Arguments.Cast<INode>());
            }
            else if (node is LocalVariableDeclaration)
                ret.AddRange(node.As<LocalVariableDeclaration>().Variables.Select(o => (INode)o.Initializer));
            else if (node is TryCatchStatement)
                ret.AddRange(node.As<TryCatchStatement>().CatchClauses.Cast<INode>().Concat(new INode[] { node.As<TryCatchStatement>().StatementBlock }));
            else if (node is AssignmentExpression)
            {
                ret.Add(node.As<AssignmentExpression>().Left);
                ret.Add(node.As<AssignmentExpression>().Right);
            }
            else if (node is BinaryOperatorExpression)
            {
                ret.Add(node.As<BinaryOperatorExpression>().Left);
                ret.Add(node.As<BinaryOperatorExpression>().Right);
            }
            else if (node is MemberReferenceExpression)
                ret.Add(node.As<MemberReferenceExpression>().TargetObject);
            else if (node is ForeachStatement)
                ret.AddRange(new INode[] { node.As<ForeachStatement>().Expression, node.As<ForeachStatement>().NextExpression });
            else if (node is SwitchSection)
                ret.AddRange(node.As<SwitchSection>().SwitchLabels.Cast<INode>());
            else if (node is CaseLabel)
                ret.Add(node.As<CaseLabel>().ToExpression);
            else if (node is ObjectCreateExpression)
            {
                var create = node.As<ObjectCreateExpression>();
                ret.AddRange(create.Parameters.Cast<INode>());
                ret.Add(create.ObjectInitializer);
            }
            else if (node is SwitchStatement)
            {
                var s = node.As<SwitchStatement>();
                ret.Add(s.SwitchExpression);
                ret.AddRange(s.SwitchSections.Cast<INode>());
            }
            else if (node is IfElseStatement)
            {
                var n = node.As<IfElseStatement>();
                ret.AddRange(n.TrueStatement.Cast<INode>());
                ret.AddRange(n.FalseStatement.Cast<INode>());
                ret.AddRange(n.ElseIfSections.Cast<INode>());
            }
            else if (node is ReturnStatement)
                ret.Add(node.As<ReturnStatement>().Expression);
            else if (node is ThrowStatement)
                ret.Add(node.As<ThrowStatement>().Expression);
            else if (node is CollectionInitializerExpression)
                ret.AddRange(node.As<CollectionInitializerExpression>().CreateExpressions.Cast<INode>());
            else if (node is ConstructorDeclaration)
                ret.Add(node.As<ConstructorDeclaration>().Body);
            else if (node is TypeOfIsExpression)
                ret.Add(node.As<TypeOfIsExpression>().Expression);
            else if (node is DoLoopStatement)
                ret.Add(node.As<DoLoopStatement>().Condition);
            else if (node is CatchClause)
            {
                ret.Add(node.As<CatchClause>().Condition);
                ret.Add(node.As<CatchClause>().StatementBlock);
            }
            else if (node is LockStatement)
                ret.Add(node.As<LockStatement>().LockExpression);
            else if (node is UsingStatement)
                ret.Add(node.As<UsingStatement>().ResourceAcquisition);
            else if (node is DelegateDeclaration)
                ret.AddRange(node.As<DelegateDeclaration>().Parameters.Cast<INode>());
            else if (node is ParameterDeclarationExpression)
                ret.Add(node.As<ParameterDeclarationExpression>().DefaultValue);
            else if (node is PropertyGetSetRegion)
                ret.Add(node.As<PropertyGetSetRegion>().Block);
            else if (node is BlockStatement || node is IdentifierExpression || node is TypeDeclaration || node is PrimitiveExpression || node is ThisReferenceExpression || node is BaseReferenceExpression || node is ContinueStatement || node is BreakStatement || node is TypeReferenceExpression || node is EmptyStatement || node is NamedArgumentExpression) { }
            else if (node.GetType().Name == "NullExpression") { }

            else throw new Exception("Need handler for " + node.GetType().Name);

            return ret;
        }

        public static IEnumerable<INode> AllLogicalChildren(this INode node)
        {
            foreach (var child in node.LogicalChildren())
            {
                yield return child;

                foreach (var c2 in child.AllLogicalChildren())
                    yield return c2;
            }
        }

        public static IEnumerable<TypeReference> ReferencesTypes(this INode node)
        {
            if (node is ObjectCreateExpression)
                return new TypeReference[] { node.As<ObjectCreateExpression>().CreateType };
            else if (node is LocalVariableDeclaration)
                return new TypeReference[] { node.As<LocalVariableDeclaration>().TypeReference };
            else if (node is PropertyDeclaration)
                return new TypeReference[] { node.As<PropertyDeclaration>().TypeReference };
            else if (node is FieldDeclaration)
                return new TypeReference[] { node.As<FieldDeclaration>().TypeReference };
            else if (node is CatchClause)
                return new TypeReference[] { node.As<CatchClause>().TypeReference };
            else if (node is MethodDeclaration)
            {
                var method = node.As<MethodDeclaration>();
                return method.Parameters.Select(o => o.TypeReference).Concat(new TypeReference[] { method.TypeReference });
            }
            else if (node is TypeDeclaration)
            {
                var t = node.As<TypeDeclaration>();
                return t.BaseTypes.Concat(t.Templates.SelectMany(o => o.Bases));
            }
            else if (node is MemberReferenceExpression)
            {
                var m = node.As<MemberReferenceExpression>();
                var trans = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Method, m.MemberName, m.TargetObject) as Translations.Method;

                if (trans != null && trans.IsExtensionMethod)
                    return new TypeReference[] { new TypeReference(trans.ExtensionNamespace) };
                else if (m.TargetObject is IdentifierExpression)
                    return new TypeReference[] { new TypeReference(m.TargetObject.As<IdentifierExpression>().Identifier) }; //warning: this will pick up false positives
                else
                    return new TypeReference[] { };
            }
            else
                return new TypeReference[] { };
        }

        public static IEnumerable<T> RemoveNull<T>(this IEnumerable<T> a) where T : class
        {
            return a.Where(o => o != null);
        }
    }
}
