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
	static class WriteLiteralExpression
	{
		public static void Go(HaxeWriter writer, LiteralExpressionSyntax expression)
		{
			var str = expression.ToString();

			if (str.StartsWith("@"))
				str = "\"" + str.RemoveFromStartOfString("@\"").RemoveFromEndOfString("\"").Replace("\\", "\\\\").Replace("\"\"", "\\\"") + "\"";
			
			if (expression.Token.Kind() == SyntaxKind.CharacterLiteralToken)
			{
				//chars just get written as integers
                
				str = ((int)(char)expression.Token.Value).ToString();
			}

			if (str.EndsWith("f") && !str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				str = str.Substring(0, str.Length - 1);

            if (str.EndsWith("L"))
                str = str.Substring(0, str.Length - 1);


			writer.Write(str);
		}

        public static string FromObject(object obj)
        {
            if (obj == null)
                return "null";
            else if (obj is bool)
                return obj.ToString().ToLower();
            else if (obj is string)
                return "\"" + obj.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            else
                return obj.ToString();

        }

	}
}
