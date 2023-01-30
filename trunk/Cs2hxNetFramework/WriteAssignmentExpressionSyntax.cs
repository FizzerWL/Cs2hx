using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Cs2hx
{
    static class WriteAssignmentExpressionSyntax
    {
        internal static void Go(HaxeWriter writer, AssignmentExpressionSyntax assignmentExpressionSyntax)
        {
            //check for invocation of overloaded operator
            var symbolInfo = Program.GetModel(assignmentExpressionSyntax).GetSymbolInfo(assignmentExpressionSyntax);
            if (symbolInfo.Symbol != null && symbolInfo.Symbol is IMethodSymbol)
            {
                var method = (IMethodSymbol)symbolInfo.Symbol;
                var type = Program.GetModel(assignmentExpressionSyntax).GetTypeInfo(assignmentExpressionSyntax).Type;
                if (method.Name.StartsWith("op_") && !type.ContainingNamespace.FullNameWithDot().StartsWith("System."))
                {
                    WriteOverloadedOperatorInvocation(writer, assignmentExpressionSyntax, method, type);
                    return;
                }
            }

            WriteBinaryExpression.Go(writer, assignmentExpressionSyntax.Left, assignmentExpressionSyntax.OperatorToken, assignmentExpressionSyntax.Right);
        }



        private static void WriteOverloadedOperatorInvocation(HaxeWriter writer, AssignmentExpressionSyntax expression, IMethodSymbol method, ITypeSymbol type)
        {
            Core.Write(writer, expression.Left);
            writer.Write(" = ");
            writer.Write(type.ContainingNamespace.FullNameWithDot().ToLower());
            writer.Write(type.Name);
            writer.Write(".");
            writer.Write(OverloadResolver.MethodName(method));
            writer.Write("(");
            Core.Write(writer, expression.Left);
            writer.Write(", ");
            Core.Write(writer, expression.Right);
            writer.Write(")");
        }
    }
}
