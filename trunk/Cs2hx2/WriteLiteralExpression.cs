using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteLiteralExpression
	{
		public static void Go(HaxeWriter writer, LiteralExpressionSyntax expression)
		{
			var str = expression.ToString();

			if (str.StartsWith("@"))
				str = str.Substring(1).Replace("\\", "\\\\");


			writer.Write(str);
		}

	}
}
