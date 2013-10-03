using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class WriteMethod
    {
        public static void Go(HaxeWriter writer, MethodDeclarationSyntax method)
        {
			var methodSymbol = TypeState.Instance.GetModel(method).GetDeclaredSymbol(method);

            writer.WriteIndent();

            if ((method.Identifier.ValueText != "ToString" || !TypeState.Instance.DerivesFromObject) && method.Modifiers.Any(SyntaxKind.OverrideKeyword))
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
				writer.Write(TypeProcessor.ConvertTypeWithColon(parameter.Type));

				if (parameter.Default != null)
				{
					writer.Write(" = ");
					Core.Write(writer, parameter.Default.Value);
				}

                parameterNumber++;
            }

            writer.Write(")");
            writer.Write(TypeProcessor.ConvertTypeWithColon(method.ReturnType));

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
