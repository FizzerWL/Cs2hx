using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{

	public class ExpressionOrString
	{
		public ExpressionSyntax Expression;
		public string String;

		public void Write(HaxeWriter writer)
		{
			if (this.String != null)
				writer.Write(this.String);
			else
				Core.Write(writer, this.Expression);
		}
	}
}
