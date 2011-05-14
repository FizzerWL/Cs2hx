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

        public static IEnumerable<INode> LogicalChildren(INode node)
        {
            if (node is ExpressionStatement)
                return new INode[] { node.As<ExpressionStatement>().Expression };
            else if (node is BlockStatement)
                return node.Children;
            else
            {
                var t = node.GetType();
                var embeddedStatement = t.GetProperty("EmbeddedStatement");
                var expression = t.GetProperty("Expression");

                if (embeddedStatement != null)
                    return new INode[] { (INode)embeddedStatement.GetValue(node, null) };
                else if (expression != null)
                    return new INode[] { (INode)expression.GetValue(node, null) };
                else
                    return node.Children;
            }
        }
    }
}
