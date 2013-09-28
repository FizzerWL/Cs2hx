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

			var typeStr = TypeProcessor.GenericTypeName(TypeState.Instance.GetModel(foreachStatement).GetTypeInfo(foreachStatement.Expression).ConvertedType);

			if (typeStr == "System.Collections.Generic.Dictionary<,>" || typeStr == "System.Collections.Generic.HashSet<>")
				writer.Write(".Values()");
			
			writer.Write(")\r\n");
			writer.WriteOpenBrace();
			Core.Write(writer, foreachStatement.Statement);
			writer.WriteCloseBrace();
		}
	}
}
