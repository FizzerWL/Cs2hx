using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class WriteMethods
    {
        public static void Go(HaxeWriter writer, IEnumerable<MethodDeclarationSyntax> methods, bool typeDerivesFromObject)
        {
            foreach (var overloadedGroup in methods.GroupBy(o => o.Identifier.ValueText))
            {
                //Find the primary method
                var method = overloadedGroup.First(o => o.ParameterList.Parameters.Count == overloadedGroup.Max(m => m.ParameterList.Parameters.Count));

                var defaultParameters = Enumerable.Range(0, method.ParameterList.Parameters.Count).Select(o => (string)null).ToList();

                foreach (var overload in overloadedGroup.Where(o => o != method))
                {
                    Action err = () => { throw new Exception("Overloads must not do anything other than call the primary method: " + Utility.Descriptor(overload)); };
                    //Each overload must resolve to the primary method
                    if (overload.Body.Statements.Count > 1)
                        err();
                    SyntaxNode stmt = overload.Body.Statements.Single();

                    if (stmt is ReturnStatementSyntax)
                        stmt = stmt.As<ReturnStatementSyntax>().Expression;
                    else
                    {
                        if (!(stmt is ExpressionStatementSyntax))
                            err();
                        stmt = stmt.As<ExpressionStatementSyntax>().Expression;
                    }

                    
                    if (!(stmt is InvocationExpressionSyntax))
                        err();
                    var args = stmt.As<InvocationExpressionSyntax>().ArgumentList.Arguments;
                    if (args.Count != method.ParameterList.Parameters.Count)
                        err();
                    for (int i = 0; i < args.Count; i++)
                    {
                        if (args[i].Expression is LiteralExpressionSyntax)
                            defaultParameters[i] = TypeProcessor.HaxeLiteral(args[i].As<LiteralExpressionSyntax>());
                        else
                            err();
                    }
                }

                writer.WriteIndent();

                if ((method.Identifier.ValueText != "ToString" || !typeDerivesFromObject) && method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                    writer.Write("override ");
                if (method.Modifiers.Any(SyntaxKind.PublicKeyword) || method.Modifiers.Any(SyntaxKind.ProtectedKeyword) || method.Modifiers.Any(SyntaxKind.InternalKeyword))
                    writer.Write("public ");
                if (method.Modifiers.Any(SyntaxKind.PrivateKeyword))
                    writer.Write("private ");
                if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
                    writer.Write("static ");

                writer.Write("function ");
                writer.Write(method.Identifier.ValueText == "ToString" ? "toString" : method.Identifier.ValueText);

                if (method.TypeParameterList != null)
                {
                    writer.Write("<");
                    writer.Write(string.Join(", ", method.TypeParameterList.Parameters.Select(o => o.Identifier.ValueText)));
                    writer.Write(">");
                }

                writer.Write("(");

                var parameterNumber = 0;
                foreach (var parameter in method.ParameterList.Parameters)
                {
                    if (parameter.Modifiers.Any(SyntaxKind.OutKeyword))
                        throw new Exception("out is not supported: " + Utility.Descriptor(method));
                    if (parameter.Modifiers.Any(SyntaxKind.RefKeyword))
                        throw new Exception("ref is not supported: " + Utility.Descriptor(method));

                    if (parameterNumber > 0)
                        writer.Write(", ");

                    writer.Write(parameter.Identifier.ValueText);
                    writer.Write(":" + TypeProcessor.ConvertType(parameter.Type));

                    var def = defaultParameters[parameterNumber];
                    if (def != null)
                        writer.Write(" = " + def);

                    parameterNumber++;
                }

                writer.Write("):");
                writer.Write(TypeProcessor.ConvertType(method.ReturnType));

                if (method.Modifiers.Any(SyntaxKind.AbstractKeyword))
                {
                    writer.WriteLine();
                    writer.WriteOpenBrace();
                    writer.WriteLine("throw new Exception(\"Abstract item called\");");
                    if (method.ReturnType.ToString() != "void")
                        writer.WriteLine("return " + TypeProcessor.DefaultValue(method.ReturnType) + ";");
                    writer.WriteCloseBrace();
                }
                else if (method.Body == null)
                    writer.Write(";\r\n"); //interface methods wind up here
                else
                {
                    writer.WriteLine();
                    writer.WriteOpenBrace();
                    foreach (var statement in method.Body.Statements)
						Core.Write(writer, statement);
                    writer.WriteCloseBrace();
                }
            }
        }
    }
}
