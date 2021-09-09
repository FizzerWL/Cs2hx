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
	static class WriteDefaultExpression
	{
		public static void Go(HaxeWriter writer, DefaultExpressionSyntax node)
		{
			//null works as a default for all types in haxe, even template types when they're primitives
			writer.Write("null");
		}
	}
}
