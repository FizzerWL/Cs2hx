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

	public class TransformedArgument
	{
		//Either String will be populated, or Argument will be. Never both.
		public readonly string StringOpt;
		public readonly ArgumentSyntax ArgumentOpt;


		public TransformedArgument(ArgumentSyntax argument)
		{
			this.ArgumentOpt = argument;
		}

		public TransformedArgument(string str)
		{
			this.StringOpt = str;
		}

		public void Write(HaxeWriter writer)
		{
			if (this.StringOpt != null)
				writer.Write(this.StringOpt);
			else
				Core.Write(writer, this.ArgumentOpt.Expression);
		}
	}
}
