using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteElementAccessExpression
	{
		public static void Go(HaxeWriter writer, ElementAccessExpressionSyntax expression)
		{
			if (expression.ArgumentList.Arguments.Count != 1)
				throw new Exception("Multiple element access not supported in C#");

			Core.Write(writer, expression.Expression);

			var typeStr = TypeProcessor.GenericTypeName(Program.GetModel(expression).GetTypeInfo(expression.Expression).ConvertedType);
			if (typeStr == "System.Collections.Generic.Dictionary<,>" || typeStr == "System.Collections.Generic.HashSet<>")
			{
				writer.Write(".GetValue(");
				Core.Write(writer, expression.ArgumentList.Arguments.Single().Expression);
				writer.Write(")");
			}
			else
			{
				writer.Write("[");
				Core.Write(writer, expression.ArgumentList.Arguments.Single().Expression);
				writer.Write("]");
			}
		}
	}
}
