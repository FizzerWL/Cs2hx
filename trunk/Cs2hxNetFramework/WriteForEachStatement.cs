using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
			WriteEnumerator(writer, foreachStatement.Expression, Program.GetModel(foreachStatement).GetTypeInfo(foreachStatement.Expression).Type, false);
			
			
			writer.Write(")\r\n");
			writer.WriteOpenBrace();

			if (foreachStatement.Statement is BlockSyntax)
				foreach (var statement in foreachStatement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, statement);
			else
				Core.Write(writer, foreachStatement.Statement);

			writer.WriteCloseBrace();
		}

		private static void WriteEnumerator(HaxeWriter writer, ExpressionSyntax expression, ITypeSymbol type, bool isFirstParameterToExtensionMethod)
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
                var haxeType = TypeProcessor.ConvertType(type);

                if (haxeType == "haxe.io.Bytes")
                    throw new Exception("Cannot use byte[] as an enumerable.  Consider using a for loop instead " + Utility.Descriptor(expression));

                //Array types support enumerating natively.  For other types, we append GetEnumerator so they can be enumerated on. (TODO: Use haxe iterator system instead, but to do this we'd have to change our hack of using Arrays all over the place).  We check type.Name != "Array" as a special case for the System.Array type for which ConvertType returns null, yet haxe supports enumerating natively.  
                if ((haxeType == null || !haxeType.StartsWith("Array")) && type.Name != "Array")
                {
                    if (isFirstParameterToExtensionMethod)
                    {
                        //When we're being called as the first parameter to an extension method, we also have to take care not to call .GetEnumerator() on something that's null. Since we could be calling an extension method on this, and in C# calling extension methods on something that's null will not generate a nullref exception.
                        writer.Write("Cs2Hx.GetEnumeratorNullCheck(");
                        Core.Write(writer, expression);
                        writer.Write(")");
                    }
                    else
                    {
                        Core.Write(writer, expression);
                        writer.Write(".GetEnumerator()");
                    }
                }
                else
                    Core.Write(writer, expression);
            }
		}

		public static void CheckWriteEnumerator(HaxeWriter writer, ExpressionSyntax expression, bool isFirstParameterToExtensionMethod)
		{
			var type = Program.GetModel(expression).GetTypeInfo(expression);

			if (type.ConvertedType == null || type.Type == null)
				Core.Write(writer, expression);
			else
			{

				if (type.ConvertedType.Name == "IEnumerable" && type.ConvertedType.ContainingNamespace.FullName().StartsWith("System.Collections"))
					WriteEnumerator(writer, expression, type.Type, isFirstParameterToExtensionMethod);
				else
					Core.Write(writer, expression);

			}
		}
	}
}
