using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class WriteInvocationExpression
    {
        public static void Go(HaxeWriter writer, InvocationExpressionSyntax invocationExpression)
        {
            Core.WriteStatement(writer, invocationExpression.Expression);


            writer.Write("(");
            var firstArg = true;
            foreach (var arg in invocationExpression.ArgumentList.Arguments)
            {
                if (firstArg)
                    firstArg = false;
                else
                    writer.Write(", ");

                Core.WriteStatement(writer, arg);
            }


            writer.Write(")");
        }
    }
}
