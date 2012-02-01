using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class Core
    {
        public static void WriteStatement(HaxeWriter writer, SyntaxNode statement)
        {
            if (statement is ExpressionStatementSyntax)
                WriteExpressionStatement(writer, statement.As<ExpressionStatementSyntax>());
            else if (statement is InvocationExpressionSyntax)
                WriteInvocationExpression.Go(writer, statement.As<InvocationExpressionSyntax>());
            else
                throw new NotImplementedException();
        }

        public static void WriteExpressionStatement(HaxeWriter writer, ExpressionStatementSyntax expressionStatement)
        {
            writer.WriteIndent();

            WriteStatement(writer, expressionStatement.Expression);

            writer.Write(";\r\n");
        }

        
    }
}
