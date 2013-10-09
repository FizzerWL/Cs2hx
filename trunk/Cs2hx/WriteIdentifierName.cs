using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteIdentifierName
	{
		public static void Go(HaxeWriter writer, IdentifierNameSyntax identifier, bool byRef = false)
		{
			writer.Write(identifier.ToString());

			if (!byRef)
			{
				var symbol = Program.GetModel(identifier).GetSymbolInfo(identifier).Symbol;
				if (Program.RefOutSymbols.ContainsKey(symbol))
					writer.Write(".Value");
			}
		}
	}
}
