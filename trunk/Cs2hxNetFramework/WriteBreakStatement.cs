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
	static class WriteBreakStatement
	{
		public static void Go(HaxeWriter writer, BreakStatementSyntax statement)
		{
            //Traverse up to figure out what we're breaking from.  If we're breaking from a loop, it's fine.  However, if we're breaking from a switch statement, throw an error as haxe doesn't allow this.
            var breakingFrom = statement.Parent;
            while (!(breakingFrom is WhileStatementSyntax || breakingFrom is ForStatementSyntax || breakingFrom is DoStatementSyntax || breakingFrom is ForEachStatementSyntax))
            {
                if (breakingFrom is SwitchStatementSyntax)
                    throw new Exception("Cannot \"break\" from within a switch statement. " + Utility.Descriptor(statement));
               
                breakingFrom = breakingFrom.Parent;
            }


			writer.WriteLine("break;");
		}
	}
}
