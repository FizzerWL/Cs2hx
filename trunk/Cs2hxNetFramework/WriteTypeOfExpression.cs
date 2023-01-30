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
	static class WriteTypeOfExpression
	{
		public static void Go(HaxeWriter writer, TypeOfExpressionSyntax expression)
		{
			throw new Exception("typeof is not supported unless part of Enum.Parse or Enum.GetValues " + Utility.Descriptor(expression));
			//writer.Write("typeof(");
			//writer.Write(TypeProcessor.ConvertType(expression.Type));
			//writer.Write(")");
		}
	}
}
