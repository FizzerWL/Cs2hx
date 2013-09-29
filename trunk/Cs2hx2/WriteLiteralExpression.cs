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
				str = "\"" + str.RemoveFromStartOfString("@\"").RemoveFromEndOfString("\"").Replace("\\", "\\\\").Replace("\"\"", "\\\"") + "\"";
			
			if (str.StartsWith("'") && str.EndsWith("'"))
			{
				//chars just get written as integers

				str = str.Substring(1, str.Length - 2);

				if (str.StartsWith("\\"))
					str = str.Substring(1);

				if (str.Length != 1)
					throw new Exception("Unexpected char string: " + str);
				str = ((int)str[0]).ToString();
			}


			writer.Write(str);
		}

	}
}
