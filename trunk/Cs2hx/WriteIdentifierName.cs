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
		public static void Go(HaxeWriter writer, IdentifierNameSyntax identifier)
		{
			writer.Write(identifier.ToString());
		}
	}
}
