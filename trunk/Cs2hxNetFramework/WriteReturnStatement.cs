using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	static class WriteReturnStatement
	{
		public static void Go(HaxeWriter writer, ReturnStatementSyntax statement)
		{
			writer.WriteIndent();
			writer.Write("return");

            if (statement.Expression != null)
            {
                writer.Write(" ");
                Core.Write(writer, statement.Expression);
            }
            else if (IsPropertySetter(statement))
            {
                //When returning from a property setter, C# just uses a simple "return;" whereas haxe requires a return value, which should always be the same value that was passed in.
                writer.Write(" value");
            }


			writer.Write(";\r\n");
		}

        /// <summary>
        /// Returns true if this return statement is returning from a property setter
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        private static bool IsPropertySetter(ReturnStatementSyntax statement)
        {
            var node = statement.Parent;

            while (true)
            {
                if (node is AccessorDeclarationSyntax && node.As<AccessorDeclarationSyntax>().Kind() == SyntaxKind.SetAccessorDeclaration)
                    return true;

                if (node is SimpleLambdaExpressionSyntax)
                    return false; //we're returning from a lambda, which may be inside of a set accessor, so make sure we don't count this return as returning from the property
                node = node.Parent;
                if (node == null)
                    return false;
            }
        }
    }
}
