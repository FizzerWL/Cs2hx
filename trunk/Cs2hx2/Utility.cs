using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    public static class Utility
    {
        public static T As<T>(this object o)
        {
            return (T)o;
        }

		public static string SubstringAfterLast(this string s, char c)
		{
			int i = s.LastIndexOf(c);
			if (i == -1)
				throw new Exception("char not found");
			return s.Substring(i + 1);
		}

		public static string SubstringBeforeLast(this string s, char c)
		{
			int i = s.LastIndexOf(c);
			if (i == -1)
				throw new Exception("char not found");
			return s.Substring(0, i);
		}

		public static MethodSymbol UnReduce(this MethodSymbol methodSymbol)
		{
			while (methodSymbol.ReducedFrom != null)
				methodSymbol = methodSymbol.ReducedFrom;

			return methodSymbol;
		}

        //public static MethodDeclaration GetMethod(INode statement)
        //{
        //    while (!(statement is MethodDeclaration))
        //    {
        //        if (statement.Parent == null)
        //            return null;
        //        statement = statement.Parent;
        //    }

        //    return statement.As<MethodDeclaration>();
        //}

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



        ///// <summary>
        ///// Identifies the type of the passed variable declaration
        ///// </summary>
        ///// <param name="localVariableDeclaration"></param>
        ///// <returns></returns>
        //public static TypeReference DetermineType(LocalVariableDeclaration localVariableDeclaration)
        //{
        //    if (localVariableDeclaration.TypeReference.Type == "var")
        //    {
        //        //If var is used, try to identify the type by the right-hand identifier
        //        foreach (var initializers in localVariableDeclaration.Variables)
        //        {
        //            if (initializers.Initializer is ObjectCreateExpression)
        //                return initializers.Initializer.As<ObjectCreateExpression>().CreateType;
        //            else if (initializers.Initializer is CastExpression)
        //                return initializers.Initializer.As<CastExpression>().CastTo;
        //        }
        //    }

        //    return localVariableDeclaration.TypeReference;
        //}

        ///// <summary>
        ///// Attempts to determine the type of an identity by looking up the call tree to find a local variable with its name
        ///// </summary>
        ///// <param name="identifier"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static bool TryFindType(IdentifierExpression identifier, out TypeReference type)
        //{
        //    return TryFindType(identifier.Identifier, identifier, out type);
        //}

        //public static bool TryFindType(string identifier, INode startAt, out TypeReference type)
        //{
        //    if (startAt == null)
        //    {
        //        type = null;
        //        return false;
        //    }

        //    if (startAt is MethodDeclaration)
        //    {
        //        //Check to see if it's a parameter
        //        var method = startAt.As<MethodDeclaration>();
        //        var prm = method.Parameters.SingleOrDefault(o => o.ParameterName == identifier);

        //        if (prm != null)
        //        {
        //            type = prm.TypeReference;
        //            return true;
        //        }
        //    }
        //    else if (startAt is ForeachStatement)
        //    {
        //        //Check to see if it's an iterator variable
        //        var forEach = startAt.As<ForeachStatement>();
        //        if (forEach.VariableName == identifier)
        //        {
        //            type = forEach.TypeReference;
        //            return true;
        //        }
        //    }

        //    //Walk up and back
        //    for (int i = startAt.Parent.Children.IndexOf(startAt) - 1; i >= 0; i--)
        //    {
        //        var t = startAt.Parent.Children[i];
        //        if (t is LocalVariableDeclaration)
        //        {
        //            var lvd = t.As<LocalVariableDeclaration>();
        //            foreach (var declaration in lvd.Variables)
        //            {
        //                if (declaration.Name == identifier)
        //                {
        //                    type = DetermineType(lvd);
        //                    return true;
        //                }
        //            }
        //        }
        //        else if (t is FieldDeclaration)
        //        {
        //            var fields = t.As<FieldDeclaration>();

        //            if (fields.Fields.Any(o => o.Name == identifier))
        //            {
        //                type = fields.TypeReference;
        //                return true;
        //            }
        //        }
        //    }

        //    if (startAt.Parent.Parent != null && TryFindType(identifier, startAt.Parent, out type))
        //        return true;

        //    type = null;
        //    return false;
        //}



        public static string Descriptor(SyntaxNode node)
        {
            var sb = new StringBuilder();
			sb.Append(node.Span.ToString() + " ");

            while (node != null)
            {
				

                if (node is BaseTypeDeclarationSyntax)
                    sb.Append("Type: " + node.As<BaseTypeDeclarationSyntax>().Identifier.ValueText + ", ");
                else if (node is MethodDeclarationSyntax)
                    sb.Append("Method: " + node.As<MethodDeclarationSyntax>().Identifier.ValueText + ", ");
                else if (node is PropertyDeclarationSyntax)
                    sb.Append("Property: " + node.As<PropertyDeclarationSyntax>().Identifier.ValueText + ", ");
                node = node.Parent;
            }

            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        //public static IEnumerable<INode> RecurseAllChildren(INode node)
        //{
        //    var ret = new List<INode>();
        //    ret.Add(node);
        //    foreach (var c in LogicalChildren(node))
        //        ret.AddRange(RecurseAllChildren(c));
        //    return ret;
        //}

        //public static IEnumerable<INode> LogicalChildren(this INode node)
        //{
        //    var ret = node.Children.ToList();

        //    if (node is StatementWithEmbeddedStatement)
        //        ret.Add(node.As<StatementWithEmbeddedStatement>().EmbeddedStatement);



        //    if (node is ExpressionStatement)
        //        ret.Add(node.As<ExpressionStatement>().Expression);
        //    else if (node is CastExpression)
        //        ret.Add(node.As<CastExpression>().Expression);
        //    else if (node is ParenthesizedExpression)
        //        ret.Add(node.As<ParenthesizedExpression>().Expression);
        //    else if (node is ElseIfSection)
        //        ret.Add(node.As<ElseIfSection>().Condition);
        //    else if (node is IndexerExpression)
        //    {
        //        ret.Add(node.As<IndexerExpression>().TargetObject);
        //        ret.AddRange(node.As<IndexerExpression>().Indexes.Cast<INode>());
        //    }
        //    else if (node is ArrayCreateExpression)
        //    {
        //        ret.AddRange(node.As<ArrayCreateExpression>().Arguments.Cast<INode>());
        //        ret.Add(node.As<ArrayCreateExpression>().ArrayInitializer);
        //    }
        //    else if (node is LambdaExpression)
        //    {
        //        ret.Add(node.As<LambdaExpression>().ExpressionBody);
        //        ret.Add(node.As<LambdaExpression>().StatementBody);
        //    }
        //    else if (node is UnaryOperatorExpression)
        //        ret.Add(node.As<UnaryOperatorExpression>().Expression);
        //    else if (node is PropertyDeclaration)
        //    {
        //        ret.Add(node.As<PropertyDeclaration>().GetRegion);
        //        ret.Add(node.As<PropertyDeclaration>().SetRegion);
        //    }
        //    else if (node is ConditionalExpression)
        //    {
        //        ret.Add(node.As<ConditionalExpression>().TrueExpression);
        //        ret.Add(node.As<ConditionalExpression>().FalseExpression);
        //    }
        //    else if (node is ForStatement)
        //    {
        //        ret.Add(node.As<ForStatement>().Condition);
        //        ret.AddRange(node.As<ForStatement>().Initializers.Cast<INode>());
        //        ret.AddRange(node.As<ForStatement>().Iterator.Cast<INode>());
        //    }
        //    else if (node is FieldDeclaration)
        //        ret.AddRange(node.As<FieldDeclaration>().Fields.Cast<INode>());
        //    else if (node is VariableDeclaration)
        //    {
        //        ret.Add(node.As<VariableDeclaration>().FixedArrayInitialization);
        //        ret.Add(node.As<VariableDeclaration>().Initializer);
        //    }
        //    else if (node is MethodDeclaration)
        //        ret.Add(node.As<MethodDeclaration>().Body);
        //    else if (node is InvocationExpression)
        //    {
        //        ret.Add(node.As<InvocationExpression>().TargetObject);
        //        ret.AddRange(node.As<InvocationExpression>().Arguments.Cast<INode>());
        //    }
        //    else if (node is LocalVariableDeclaration)
        //        ret.AddRange(node.As<LocalVariableDeclaration>().Variables.Select(o => (INode)o.Initializer));
        //    else if (node is TryCatchStatement)
        //        ret.AddRange(node.As<TryCatchStatement>().CatchClauses.Cast<INode>().Concat(new INode[] { node.As<TryCatchStatement>().StatementBlock }));
        //    else if (node is AssignmentExpression)
        //    {
        //        ret.Add(node.As<AssignmentExpression>().Left);
        //        ret.Add(node.As<AssignmentExpression>().Right);
        //    }
        //    else if (node is BinaryOperatorExpression)
        //    {
        //        ret.Add(node.As<BinaryOperatorExpression>().Left);
        //        ret.Add(node.As<BinaryOperatorExpression>().Right);
        //    }
        //    else if (node is MemberReferenceExpression)
        //        ret.Add(node.As<MemberReferenceExpression>().TargetObject);
        //    else if (node is ForeachStatement)
        //        ret.AddRange(new INode[] { node.As<ForeachStatement>().Expression, node.As<ForeachStatement>().NextExpression });
        //    else if (node is SwitchSection)
        //        ret.AddRange(node.As<SwitchSection>().SwitchLabels.Cast<INode>());
        //    else if (node is CaseLabel)
        //    {
        //        var lbl = node.As<CaseLabel>();
        //        ret.Add(lbl.ToExpression);
        //        ret.Add(lbl.Label);
        //    }
        //    else if (node is ObjectCreateExpression)
        //    {
        //        var create = node.As<ObjectCreateExpression>();
        //        ret.AddRange(create.Parameters.Cast<INode>());
        //        ret.Add(create.ObjectInitializer);
        //    }
        //    else if (node is SwitchStatement)
        //    {
        //        var s = node.As<SwitchStatement>();
        //        ret.Add(s.SwitchExpression);
        //        ret.AddRange(s.SwitchSections.Cast<INode>());
        //    }
        //    else if (node is IfElseStatement)
        //    {
        //        var n = node.As<IfElseStatement>();
        //        ret.Add(n.Condition);
        //        ret.AddRange(n.TrueStatement.Cast<INode>());
        //        ret.AddRange(n.FalseStatement.Cast<INode>());
        //        ret.AddRange(n.ElseIfSections.Cast<INode>());
        //    }
        //    else if (node is ReturnStatement)
        //        ret.Add(node.As<ReturnStatement>().Expression);
        //    else if (node is ThrowStatement)
        //        ret.Add(node.As<ThrowStatement>().Expression);
        //    else if (node is CollectionInitializerExpression)
        //        ret.AddRange(node.As<CollectionInitializerExpression>().CreateExpressions.Cast<INode>());
        //    else if (node is ConstructorDeclaration)
        //        ret.Add(node.As<ConstructorDeclaration>().Body);
        //    else if (node is TypeOfIsExpression)
        //        ret.Add(node.As<TypeOfIsExpression>().Expression);
        //    else if (node is DoLoopStatement)
        //        ret.Add(node.As<DoLoopStatement>().Condition);
        //    else if (node is CatchClause)
        //    {
        //        ret.Add(node.As<CatchClause>().Condition);
        //        ret.Add(node.As<CatchClause>().StatementBlock);
        //    }
        //    else if (node is LockStatement)
        //        ret.Add(node.As<LockStatement>().LockExpression);
        //    else if (node is UsingStatement)
        //        ret.Add(node.As<UsingStatement>().ResourceAcquisition);
        //    else if (node is DelegateDeclaration)
        //        ret.AddRange(node.As<DelegateDeclaration>().Parameters.Cast<INode>());
        //    else if (node is ParameterDeclarationExpression)
        //        ret.Add(node.As<ParameterDeclarationExpression>().DefaultValue);
        //    else if (node is PropertyGetSetRegion)
        //        ret.Add(node.As<PropertyGetSetRegion>().Block);
        //    else if (node is BlockStatement || node is IdentifierExpression || node is TypeDeclaration || node is PrimitiveExpression || node is ThisReferenceExpression || node is BaseReferenceExpression || node is ContinueStatement || node is BreakStatement || node is TypeReferenceExpression || node is EmptyStatement || node is NamedArgumentExpression) { }
        //    else if (node.GetType().Name == "NullExpression") { }

        //    else throw new Exception("Need handler for " + node.GetType().Name);

        //    return ret;
        //}

        //public static IEnumerable<INode> AllLogicalChildren(this INode node)
        //{
        //    foreach (var child in node.LogicalChildren())
        //    {
        //        yield return child;

        //        foreach (var c2 in child.AllLogicalChildren())
        //            yield return c2;
        //    }
        //}

        //public static IEnumerable<TypeReference> ReferencesTypes(this INode node)
        //{
        //    if (node is ObjectCreateExpression)
        //        return new TypeReference[] { node.As<ObjectCreateExpression>().CreateType };
        //    else if (node is LocalVariableDeclaration)
        //        return new TypeReference[] { node.As<LocalVariableDeclaration>().TypeReference };
        //    else if (node is PropertyDeclaration)
        //        return new TypeReference[] { node.As<PropertyDeclaration>().TypeReference };
        //    else if (node is FieldDeclaration)
        //        return new TypeReference[] { node.As<FieldDeclaration>().TypeReference };
        //    else if (node is CatchClause)
        //        return new TypeReference[] { node.As<CatchClause>().TypeReference };
        //    else if (node is ConstructorDeclaration)
        //        return node.As<ConstructorDeclaration>().Parameters.Select(o => o.TypeReference);
        //    else if (node is CastExpression)
        //        return new TypeReference[] { node.As<CastExpression>().CastTo };
        //    else if (node is MethodDeclaration)
        //    {
        //        var method = node.As<MethodDeclaration>();
        //        return method.Parameters.Select(o => o.TypeReference).Concat(new TypeReference[] { method.TypeReference });
        //    }
        //    else if (node is TypeDeclaration)
        //    {
        //        var t = node.As<TypeDeclaration>();
        //        return t.BaseTypes.Concat(t.Templates.SelectMany(o => o.Bases));
        //    }
        //    else if (node is MemberReferenceExpression)
        //    {
        //        var m = node.As<MemberReferenceExpression>();
        //        var trans = Translations.Translation.GetTranslation(Translations.Translation.TranslationType.Method, m.MemberName, m.TargetObject) as Translations.Method;

        //        if (trans != null && trans.IsExtensionMethod)
        //            return new TypeReference[] { new TypeReference(trans.ExtensionNamespace) };
        //        else if (m.TargetObject is IdentifierExpression)
        //            return new TypeReference[] { new TypeReference(m.TargetObject.As<IdentifierExpression>().Identifier) }; //warning: this will pick up false positives
        //        else
        //            return new TypeReference[] { };
        //    }
        //    else
        //        return new TypeReference[] { };
        //}

        //public static bool IsGenericType(TypeReference type, INode foundInExpression)
        //{
        //    var expression = foundInExpression;

        //    while (expression != null)
        //    {
        //        if (expression is MethodDeclaration)
        //        {
        //            if (expression.As<MethodDeclaration>().Templates.Any(o => o.Name == type.Type))
        //                return true;
        //        }
        //        else if (expression is TypeDeclaration)
        //        {
        //            if (expression.As<TypeDeclaration>().Templates.Any(o => o.Name == type.Type))
        //                return true;
        //        }

        //        expression = expression.Parent;
        //    }

        //    return false;
        //}

        public static Dictionary<string, string> GetCS2HXAttribute(SyntaxNode node)
        {
            AttributeSyntax attr = null;

			while (node != null)
			{
				if (node is BaseMethodDeclarationSyntax || node is BaseTypeDeclarationSyntax || node is BaseFieldDeclarationSyntax)
				{
					var list = node is BaseMethodDeclarationSyntax ? node.As<BaseMethodDeclarationSyntax>().AttributeLists 
						: node is BaseTypeDeclarationSyntax ? node.As<BaseTypeDeclarationSyntax>().AttributeLists
						: node.As<BaseFieldDeclarationSyntax>().AttributeLists;

					attr = list.SelectMany(o => o.Attributes).SingleOrDefault(o => o.Name.ToString() == "Cs2Hx");
					if (attr != null)
						break;
				}
				node = node.Parent;
			}

            if (attr == null || attr.ArgumentList == null)
                return new Dictionary<string, string>();

            return attr.ArgumentList.Arguments.ToDictionary(GetAttributeName, o => o.Expression.As<LiteralExpressionSyntax>().Token.ValueText);
        }

        private static string GetAttributeName(AttributeArgumentSyntax attr)
        {
			return attr.NameEquals.Name.ToString();
			//if (attr.NameEquals != null)
			//	return attr.NameEquals.Identifier.ValueText;
			//else if (attr.NameColon != null)
			//	return attr.NameColon.Identifier.ValueText;
			//else
			//	throw new Exception("Both NameEquals and NameColon null"); //TODO: Is this possible?
        }
		public static string RemoveFromStartOfString(this string s, string toRemove)
		{
			if (!s.StartsWith(toRemove))
				throw new Exception("Does not start string: " + s);
			return s.Substring(toRemove.Length);
		}

		public static string RemoveFromEndOfString(this string s, string toRemove)
		{
			if (!s.EndsWith(toRemove))
				throw new Exception("Does not end string: " + s);
			return s.Substring(0, s.Length - toRemove.Length);
		}
	}
}
