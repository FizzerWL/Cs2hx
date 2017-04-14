using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
    static class WriteMethod
    {
        public static void Go(HaxeWriter writer, MethodDeclarationSyntax method)
        {
			if (method.Modifiers.Any(SyntaxKind.PartialKeyword) && method.Body == null)
			{
				//We only want to render out one of the two partial methods.  If there's another, skip this one.
				if (TypeState.Instance.Partials.SelectMany(o => o.Syntax.As<ClassDeclarationSyntax>().Members)
					.OfType<MethodDeclarationSyntax>()
					.Except(method)
					.Where(o => o.Identifier.ValueText == method.Identifier.ValueText)
					.Any())
					return;
			}

			if (method.Identifier.ValueText == "GetEnumerator")
				return; //skip GetEnumerator methods -- haxe can't enumerate on objects.  TODO: Render these out, but convert them to array-returning methods

			var methodSymbol = Program.GetModel(method).GetDeclaredSymbol(method);

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

            if (method.TypeParameterList != null)
            {
                writer.Write("<");
				writer.Write(string.Join(", ", method.TypeParameterList.Parameters.Select(o => TypeParameter(o, method.ConstraintClauses))));
                writer.Write(">");
            }

            writer.Write("(");
			var deferredDefaults = new Dictionary<string, ExpressionSyntax>();

			var firstParam = true;
            foreach (var parameter in method.ParameterList.Parameters)
            {
				bool isRef = parameter.Modifiers.Any(SyntaxKind.OutKeyword) || parameter.Modifiers.Any(SyntaxKind.RefKeyword);

				if (firstParam)
					firstParam = false;
				else
                    writer.Write(", ");

                writer.Write(parameter.Identifier.ValueText);

				if (isRef)
				{
					writer.Write(":CsRef<");
					writer.Write(TypeProcessor.ConvertType(parameter.Type));
					writer.Write(">");

					Program.RefOutSymbols.TryAdd(Program.GetModel(method).GetDeclaredSymbol(parameter), null);
				}
				else
					writer.Write(TypeProcessor.ConvertTypeWithColon(parameter.Type));

				if (parameter.Default != null)
				{
					writer.Write(" = ");

					if (TypeProcessor.ConvertType(parameter.Type).StartsWith("Nullable"))
					{
						writer.Write("null");
						deferredDefaults.Add(parameter.Identifier.ValueText, parameter.Default.Value);
					}
					else 
						Core.Write(writer, parameter.Default.Value);
				}
            }

            writer.Write(")");
            writer.Write(TypeProcessor.ConvertTypeWithColon(method.ReturnType));

            if (method.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                writer.WriteLine();
                writer.WriteOpenBrace();
				writer.WriteIndent();

				if (method.ReturnType.ToString() != "void")
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

		private static bool ShouldUseOverrideKeyword(MethodDeclarationSyntax method, IMethodSymbol symbol)
		{
			if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
				return false;

			if (method.Identifier.ValueText == "ToString")
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
    }
} 
