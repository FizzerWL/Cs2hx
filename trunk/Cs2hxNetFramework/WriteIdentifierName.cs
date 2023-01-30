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
	static class WriteIdentifierName
	{
		public static void Go(HaxeWriter writer, IdentifierNameSyntax identifier, bool byRef = false)
		{
			writer.Write(identifier.ToString());

			if (!byRef)
			{
				var symbol = Program.GetModel(identifier).GetSymbolInfo(identifier).Symbol;
                if (symbol == null)
                    throw new Exception("Symbol could not be resolved.  Are you sure the C# is valid? " + Utility.Descriptor(identifier));
				if (Program.RefOutSymbols.ContainsKey(symbol))
					writer.Write(".Value");
			}
		}
	}
}
