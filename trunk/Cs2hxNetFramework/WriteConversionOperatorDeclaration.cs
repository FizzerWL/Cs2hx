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
	static class WriteConversionOperatorDeclaration
	{
		public static void Go(HaxeWriter writer, ConversionOperatorDeclarationSyntax method)
		{
			if (method.ImplicitOrExplicitKeyword.Kind() != SyntaxKind.ExplicitKeyword)
				throw new Exception("Implicit cast operators are not supported " + Utility.Descriptor(method));

			writer.WriteIndent();
			writer.Write("public static function op_Explicit_");
			writer.Write(TypeProcessor.ConvertType(method.Type));
			writer.Write("(");

			bool firstParam = true;
			foreach(var param in method.ParameterList.Parameters)
			{
				if (firstParam)
					firstParam = false;
				else
					writer.Write(", ");

				writer.Write(param.Identifier.ValueText);
				writer.Write(TypeProcessor.ConvertTypeWithColon(param.Type));
			}

			writer.Write(")");
			writer.Write(TypeProcessor.ConvertTypeWithColon(method.Type));
			writer.Write("\r\n");

			writer.WriteOpenBrace();

			foreach (var statement in method.Body.Statements)
				Core.Write(writer, statement);

			writer.WriteCloseBrace();
		}
	}
}
