using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteLocalDeclaration
	{
		public static void Go(HaxeWriter writer, LocalDeclarationStatementSyntax declaration)
		{
			foreach (var variable in declaration.Declaration.Variables)
			{
				writer.WriteIndent();
				writer.Write("var " + variable.Identifier.ToString() + TypeProcessor.ConvertTypeWithColon(declaration.Declaration.Type));

				if (variable.Initializer != null)
				{
					writer.Write(" = ");
					WriteBinaryExpression.WriteAssignment(writer, variable.Initializer.As<EqualsValueClauseSyntax>().Value);
				}

				writer.Write(";\r\n");
			}
		}
	}
}
