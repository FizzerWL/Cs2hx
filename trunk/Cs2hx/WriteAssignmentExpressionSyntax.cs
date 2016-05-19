using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
    static class WriteAssignmentExpressionSyntax
    {
        internal static void Go(HaxeWriter writer, AssignmentExpressionSyntax assignmentExpressionSyntax)
        {
            WriteBinaryExpression.Go(writer, assignmentExpressionSyntax.Left, assignmentExpressionSyntax.OperatorToken, assignmentExpressionSyntax.Right);
        }
    }
}
