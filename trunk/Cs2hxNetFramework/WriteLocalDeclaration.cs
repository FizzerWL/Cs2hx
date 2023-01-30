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
	static class WriteLocalDeclaration
	{
		public static void Go(HaxeWriter writer, LocalDeclarationStatementSyntax declaration)
		{
			foreach (var variable in declaration.Declaration.Variables)
			{
				var symbol = Program.GetModel(declaration).GetDeclaredSymbol(variable);

				var isRef = UsedAsRef(variable, symbol);

				writer.WriteIndent();
				writer.Write("var ");
				writer.Write(variable.Identifier.ValueText);

				if (isRef)
				{
					var typeStr = TypeProcessor.ConvertType(declaration.Declaration.Type);

					writer.Write(":CsRef<");
					writer.Write(typeStr);
					writer.Write(">");

					Program.RefOutSymbols.TryAdd(symbol, null);

					writer.Write(" = new CsRef<");
					writer.Write(typeStr);
					writer.Write(">(");

					if (variable.Initializer == null)
						writer.Write(TypeProcessor.DefaultValue(typeStr));
					else
						Core.Write(writer, variable.Initializer.As<EqualsValueClauseSyntax>().Value);

					writer.Write(")");
				}
				else
				{
					writer.Write(TypeProcessor.ConvertTypeWithColon(declaration.Declaration.Type));

					if (variable.Initializer != null)
					{
						writer.Write(" = ");
						Core.Write(writer, variable.Initializer.As<EqualsValueClauseSyntax>().Value);
					}
				}

				writer.Write(";\r\n");
			}
		}

		/// <summary>
		/// Determines if the passed symbol is used in any ref or out clauses
		/// </summary>
		private static bool UsedAsRef(VariableDeclaratorSyntax variable, ISymbol symbol)
		{
			SyntaxNode node = variable;
			BlockSyntax scope;
			do
				scope = (node = node.Parent) as BlockSyntax;
			while (scope == null);

			var model = Program.GetModel(variable);

			return scope.DescendantNodes().OfType<InvocationExpressionSyntax>()
				.SelectMany(o => o.ArgumentList.Arguments)
				.Where(o => o.RefOrOutKeyword.Kind() != SyntaxKind.None)
				.Any(o => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(o.Expression).Symbol, symbol));

		}
	}
}
