using System;
using System.Collections.Generic;
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
			Core.Write(writer, foreachStatement.Expression);

			WriteEnumeratorSuffix(writer, TypeState.Instance.GetModel(foreachStatement).GetTypeInfo(foreachStatement.Expression).ConvertedType);
			
			
			writer.Write(")\r\n");
			writer.WriteOpenBrace();
			Core.Write(writer, foreachStatement.Statement);
			writer.WriteCloseBrace();
		}

		public static void WriteEnumeratorSuffix(HaxeWriter writer, TypeSymbol type)
		{
			var typeStr = TypeProcessor.GenericTypeName(type);

			if (typeStr == "System.Collections.Generic.Dictionary<,>")
				writer.Write(".KeyValues()");
			else if (typeStr == "System.Collections.Generic.HashSet<>")
				writer.Write(".Values()");
			else if (typeStr == "System.Linq.IGrouping<,>")
				writer.Write(".Values()");
		}

		public static void CheckEnumeratorSuffix(HaxeWriter writer, ExpressionSyntax expression)
		{
			var type = TypeState.Instance.GetModel(expression).GetTypeInfo(expression);

			if (type.ConvertedType == null || type.Type == null)
				return;

			if (type.ConvertedType.Name == "IEnumerable" && type.ConvertedType.ContainingNamespace.ToString() == "System.Collections.Generic")
				WriteEnumeratorSuffix(writer, type.Type);
		}
	}
}
