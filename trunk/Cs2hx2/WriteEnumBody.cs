using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteEnumBody
	{
		public static void Go(HaxeWriter writer, IEnumerable<EnumMemberDeclarationSyntax> allChildren)
		{
			int lastEnumValue = 0;
			foreach (var varDeclaration in allChildren)
			{
				if (varDeclaration.EqualsValue == null)
					lastEnumValue++;
				else
					lastEnumValue = int.Parse(varDeclaration.EqualsValue.Value.ToString());

				writer.WriteLine("public static inline var " + varDeclaration.Identifier.ValueText + ":Int = " + lastEnumValue + ";");
			}
		}
	}
}
