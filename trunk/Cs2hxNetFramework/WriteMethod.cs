using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cs2hx.Translations;

namespace Cs2hx
{
    static class WriteMethod
    {
        public static void Go(HaxeWriter writer, MethodDeclarationSyntax method)
        {
            GoInternal(writer, method, method.ReturnType, method.TypeParameterList, method.ConstraintClauses);
        }

        public static void WriteOperatorDeclaration(HaxeWriter writer, OperatorDeclarationSyntax decl)
        {
            GoInternal(writer, decl, decl.ReturnType, null, null);
        }

        private static void GoInternal(HaxeWriter writer, BaseMethodDeclarationSyntax method, TypeSyntax returnType, TypeParameterListSyntax typeParameterListOpt, SyntaxList<TypeParameterConstraintClauseSyntax>? constraintClassesOpt)
        {
            var methodSymbol = Program.GetModel(method).GetDeclaredSymbol(method);

            if (method.Modifiers.Any(SyntaxKind.PartialKeyword) && method.Body == null)
			{
				//We only want to render out one of the two partial methods.  If there's another, skip this one.
				if (TypeState.Instance.Partials.SelectMany(o => o.Syntax.As<ClassDeclarationSyntax>().Members)
					.OfType<MethodDeclarationSyntax>()
					.Except(method as MethodDeclarationSyntax)
					.Where(o => o.Identifier.ValueText == methodSymbol.Name)
					.Any())
					return;
			}

            if (methodSymbol.Name == "System.Collections.IEnumerable.GetEnumerator")
                return; //we don't support the non-generic enumerator

            if (methodSymbol.Name == "GetEnumerator")
            {
                WriteGetEnumeratorFunction(writer, method, methodSymbol);
                return;
            }

            writer.WriteIndent();

            if (ShouldUseOverrideKeyword(method, methodSymbol))
                writer.Write("override ");
            if (method.Modifiers.Any(SyntaxKind.PublicKeyword) || method.Modifiers.Any(SyntaxKind.ProtectedKeyword) || method.Modifiers.Any(SyntaxKind.InternalKeyword))
                writer.Write("public ");
            if (method.Modifiers.Any(SyntaxKind.PrivateKeyword))
                writer.Write("private ");
            if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
                writer.Write("static ");

            writer.Write("function ");
			var methodName = OverloadResolver.MethodName(methodSymbol);

			if (methodName == "ToString")
				methodName = "toString";

            writer.Write(methodName);

            if (typeParameterListOpt != null)
            {
                writer.Write("<");
				writer.Write(string.Join(", ", typeParameterListOpt.Parameters.Select(o => TypeParameter(o, constraintClassesOpt))));
                writer.Write(">");
            }

            writer.Write("(");

            Dictionary<string, ExpressionSyntax> deferredDefaults;
            WriteParameters(writer, method, methodSymbol, out deferredDefaults);

            writer.Write(")");
            writer.Write(TypeProcessor.ConvertTypeWithColon(returnType));

            if (method.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                writer.WriteLine();
                writer.WriteOpenBrace();
				writer.WriteIndent();

				if (returnType.ToString() != "void")
					writer.Write("return "); //"return" the throw statement to work around haxe limitations

                writer.Write("throw new Exception(\"Abstract item called\");\r\n");
                writer.WriteCloseBrace();
            }
            else if (method.Parent is InterfaceDeclarationSyntax)
                writer.Write(";\r\n");
            else
            {
                writer.WriteLine();
                writer.WriteOpenBrace();

				foreach(var defer in deferredDefaults)
				{
					writer.WriteLine("if (" + defer.Key + " == null)");
					writer.Indent++;
					writer.WriteIndent();
					writer.Write(defer.Key);
					writer.Write(" = ");
					Core.Write(writer, defer.Value);
					writer.Write(";\r\n");
					writer.Indent--;
				}

				if (method.Body != null)
				{
					foreach (var statement in method.Body.Statements)
						Core.Write(writer, statement);

					TriviaProcessor.ProcessTrivias(writer, method.Body.DescendantTrivia());
				}

                writer.WriteCloseBrace();
            }
        }

        public static void WriteParameters(HaxeWriter writer, BaseMethodDeclarationSyntax method, IMethodSymbol methodSymbol, out Dictionary<string, ExpressionSyntax> deferredDefaults)
        {
            deferredDefaults = new Dictionary<string, ExpressionSyntax>();

            var prms = method.ParameterList.Parameters.Select(o => new TransformedArgument(o)).ToList();

            var translateOpt = MethodTranslation.Get(methodSymbol);
            if (translateOpt != null)
                prms = translateOpt.As<MethodTranslation>().TranslateParameters(prms, null).ToList();

            var firstParam = true;
            foreach (var parameter in prms)
            {
                bool isRef = parameter.ParameterOpt != null && (parameter.ParameterOpt.Modifiers.Any(SyntaxKind.OutKeyword) || parameter.ParameterOpt.Modifiers.Any(SyntaxKind.RefKeyword));

                if (parameter.StringOpt != null)
                    continue; //these are only used on invocations

                if (firstParam)
                    firstParam = false;
                else
                    writer.Write(", ");

                writer.Write(parameter.ParameterOpt.Identifier.ValueText);


                if (isRef)
                {
                    writer.Write(":CsRef<");
                    writer.Write(TypeProcessor.ConvertType(parameter.ParameterOpt.Type));
                    writer.Write(">");

                    Program.RefOutSymbols.TryAdd(Program.GetModel(method).GetDeclaredSymbol(parameter.ParameterOpt), null);
                }
                else
                    writer.Write(TypeProcessor.ConvertTypeWithColon(parameter.ParameterOpt.Type));

                if (parameter.ParameterOpt.Default != null)
                {
                    writer.Write(" = ");

                    if (TypeProcessor.ConvertType(parameter.ParameterOpt.Type).StartsWith("Nullable"))
                    {
                        writer.Write("null");
                        deferredDefaults.Add(parameter.ParameterOpt.Identifier.ValueText, parameter.ParameterOpt.Default.Value);
                    }
                    else
                        Core.Write(writer, parameter.ParameterOpt.Default.Value);
                }
            }


            int tIndex = 1;
            foreach (var genericVar in Utility.PassTypeArgsToMethod(methodSymbol))
            {
                if (firstParam)
                    firstParam = false;
                else
                    writer.Write(", ");

                writer.Write("t" + tIndex.ToString());
                writer.Write(":Class<");
                writer.Write(TypeProcessor.ConvertType(genericVar));
                writer.Write(">");
                tIndex++;
            }
        }

        private static void WriteGetEnumeratorFunction(HaxeWriter writer, BaseMethodDeclarationSyntax method, IMethodSymbol methodSymbol)
        {
            var returnType = TypeProcessor.ConvertType(methodSymbol.ReturnType);

            if (!returnType.StartsWith("system.collections.generic.IEnumerator<"))
                return; //we only support the generic IEnumerator form of GetEnumerator.  Anything else, just don't write out the method.

            var enumerableType = returnType.RemoveFromStartOfString("system.collections.generic.IEnumerator<").RemoveFromEndOfString(">");

            //We only support very simple GetEnumerator functions that pass on their call to some other collection.  The body should be like "return <expr>.GetEnumerator();", otherwise don't write out the function at all.
            if (method.Body == null)
                return;
            if (method.Body.Statements.Count > 1)
                return;
            var returnStatement = method.Body.Statements.Single() as ReturnStatementSyntax;
            if (returnStatement == null)
                return;
            var invocation = returnStatement.Expression as InvocationExpressionSyntax;
            if (invocation == null)
                return;
            var member = invocation.Expression as MemberAccessExpressionSyntax;
            if (member == null)
                return;

            var memberExpressionType = Program.GetModel(member).GetTypeInfo(member.Expression).Type;
            var memberExpressionHaxeType = TypeProcessor.ConvertType(memberExpressionType);

            writer.WriteIndent();
            writer.Write("public function iterator():Iterator<");
            writer.Write(enumerableType);
            writer.Write(">\r\n");
            writer.WriteOpenBrace();

            writer.WriteIndent();
            writer.Write("return ");
            Core.Write(writer, member.Expression);
            writer.Write(".iterator();\r\n");
            writer.WriteCloseBrace();

            //Also write out a GetEnumerator(), which returns the same thing but as an array
            writer.WriteIndent();
            writer.Write("public function GetEnumerator():Array<");
            writer.Write(enumerableType);
            writer.Write(">\r\n");
            writer.WriteOpenBrace();

            writer.WriteIndent();
            writer.Write("return ");
            Core.Write(writer, member.Expression);

            if (!memberExpressionHaxeType.StartsWith("Array<"))
                writer.Write(".GetEnumerator()");
            writer.Write(";\r\n");
            writer.WriteCloseBrace();

        }

        private static bool ShouldUseOverrideKeyword(BaseMethodDeclarationSyntax method, IMethodSymbol symbol)
		{
			if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
				return false;

			if (symbol.Name == "ToString")
				return !TypeState.Instance.DerivesFromObject;
			if (method.Modifiers.Any(SyntaxKind.NewKeyword))
				return true;

			if (method.Modifiers.Any(SyntaxKind.PartialKeyword)) //partial methods seem exempt from C#'s normal override keyword requirement, so we have to check manually to see if it exists in a base class
				return symbol.ContainingType.BaseType.GetMembers(symbol.Name).Any();

			return method.Modifiers.Any(SyntaxKind.OverrideKeyword);
		}

		public static string TypeParameter(TypeParameterSyntax prm, IEnumerable<TypeParameterConstraintClauseSyntax> constraints)
		{
			var identifier = prm.Identifier.ValueText;

			var constraint = constraints.SingleOrDefault(o => o.Name.Identifier.ValueText == identifier);

			if (constraint == null)
				return identifier;

			return identifier + ": (" + string.Join(", ", constraint.Constraints.OfType<TypeConstraintSyntax>().ToList().Select(o => TypeProcessor.ConvertType(o.Type))) + ")";
		}

        public static void WriteIndexerDeclaration(HaxeWriter writer, IndexerDeclarationSyntax decl)
        {
            foreach (var accessor in decl.AccessorList.Accessors)
            {
                writer.WriteIndent();

                if (decl.Modifiers.Any(SyntaxKind.OverrideKeyword) || decl.Modifiers.Any(SyntaxKind.NewKeyword))
                    writer.Write("override ");
                if (decl.Modifiers.Any(SyntaxKind.PublicKeyword) || decl.Modifiers.Any(SyntaxKind.ProtectedKeyword) || decl.Modifiers.Any(SyntaxKind.InternalKeyword))
                    writer.Write("public ");
                if (decl.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    writer.Write("private ");

                var isGet = accessor.Kind() == SyntaxKind.GetAccessorDeclaration;


                writer.Write("function ");
                writer.Write(isGet ? "Get" : "Set");
                writer.Write("Value_");
                writer.Write(Program.GetModel(decl).GetTypeInfo(decl.ParameterList.Parameters.Single().Type).Type.Name);
                writer.Write("(");

                foreach (var prm in decl.ParameterList.Parameters)
                {
                    writer.Write(prm.Identifier.ValueText);
                    writer.Write(TypeProcessor.ConvertTypeWithColon(prm.Type));
                }

                if (isGet)
                {
                    writer.Write(")");
                    writer.Write(TypeProcessor.ConvertTypeWithColon(decl.Type));
                }
                else
                {
                    writer.Write(", value");
                    writer.Write(TypeProcessor.ConvertTypeWithColon(decl.Type));
                    writer.Write("):Void");
                }
                writer.WriteLine();

                if (accessor.Body != null)
                    Core.Write(writer, accessor.Body);
            }
        }
    }
} 
