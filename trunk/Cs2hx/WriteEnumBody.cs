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
	static class WriteEnumBody
	{
		public static void Go(HaxeWriter writer, IEnumerable<EnumMemberDeclarationSyntax> allChildren)
		{
			int nextEnumValue = 0;

			var values = allChildren.Select(o => new { Syntax = o, Value = DetermineEnumValue(o, ref nextEnumValue) }).ToList();

			foreach (var value in values)
				writer.WriteLine("public static inline var " + value.Syntax.Identifier.ValueText + ":Int = " + value.Value + ";");

			writer.WriteLine();

			writer.WriteLine("public static function ToString(e:Int):String");
			writer.WriteOpenBrace();
			writer.WriteLine("switch (e)");
			writer.WriteOpenBrace();

			foreach (var value in values)
				writer.WriteLine("case " + value.Value + ": return \"" + value.Syntax.Identifier.ValueText + "\";");

			writer.WriteLine("default: return Std.string(e);");

			writer.WriteCloseBrace();
			writer.WriteCloseBrace();

			writer.WriteLine();
			writer.WriteLine("public static function Parse(s:String):Int");
			writer.WriteOpenBrace();
			writer.WriteLine("switch (s)");
			writer.WriteOpenBrace();

			foreach (var value in values)
				writer.WriteLine("case \"" + value.Syntax.Identifier.ValueText + "\": return " + value.Value + ";");

			writer.WriteLine("default: throw new InvalidOperationException(s);");
			writer.WriteCloseBrace();
			writer.WriteCloseBrace();

			writer.WriteLine();
			writer.WriteLine("public static function Values():Array<Int>");
			writer.WriteOpenBrace();

			writer.WriteIndent();
			writer.Write("return [");
			writer.Write(string.Join(", ", values.Select(o => o.Value.ToString())));
			writer.Write("];\r\n");
			writer.WriteCloseBrace();

		}

		private static int DetermineEnumValue(EnumMemberDeclarationSyntax syntax, ref int nextEnumValue)
		{
			if (syntax.EqualsValue == null)
				return nextEnumValue++;

			if (!int.TryParse(syntax.EqualsValue.Value.ToString(), out nextEnumValue))
				throw new Exception("Enums must be assigned with an integer " + Utility.Descriptor(syntax));

			return nextEnumValue++;
		}


	}
}
