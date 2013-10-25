using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteForEachStatement
	{
		public static void Go(HaxeWriter writer, ForEachStatementSyntax foreachStatement)
		{
			writer.WriteIndent();
			writer.Write("for (");
			writer.Write(foreachStatement.Identifier.ValueText);
			writer.Write(" in ");
			WriteEnumerator(writer, foreachStatement.Expression, Program.GetModel(foreachStatement).GetTypeInfo(foreachStatement.Expression).Type);
			
			
			writer.Write(")\r\n");
			writer.WriteOpenBrace();

			if (foreachStatement.Statement is BlockSyntax)
				foreach (var statement in foreachStatement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);
			else
				Core.Write(writer, foreachStatement.Statement);

			writer.WriteCloseBrace();
		}

		private static void WriteEnumerator(HaxeWriter writer, ExpressionSyntax expression, TypeSymbol type)
		{
			var typeStr = TypeProcessor.GenericTypeName(type);
			
			if (typeStr == "System.String")
			{
				writer.Write("Cs2Hx.ToCharArray(");
				Core.Write(writer, expression);
				writer.Write(")");
			}
			else
			{
				

				Core.Write(writer, expression);

				var haxeType = TypeProcessor.ConvertType(type);

				if (haxeType == "haxe.io.Bytes")
					throw new Exception("Cannot use byte[] as an enumerable.  Consider using a for loop instead " + Utility.Descriptor(expression));

				if (haxeType == null || !haxeType.StartsWith("Array"))
					writer.Write(".GetEnumerator()");
			}
		}

		public static void CheckWriteEnumerator(HaxeWriter writer, ExpressionSyntax expression)
		{
			var type = Program.GetModel(expression).GetTypeInfo(expression);

			if (type.ConvertedType == null || type.Type == null)
				Core.Write(writer, expression);
			else
			{

				if (type.ConvertedType.Name == "IEnumerable" && type.ConvertedType.ContainingNamespace.FullName() == "System.Collections.Generic")
					WriteEnumerator(writer, expression, type.Type);
				else
					Core.Write(writer, expression);

			}
		}
	}
}
