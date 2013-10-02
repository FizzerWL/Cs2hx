using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteContinueStatement
	{
		public static void Go(HaxeWriter writer, ContinueStatementSyntax statement)
		{
			writer.WriteLine("continue;");
		}
	}
}
