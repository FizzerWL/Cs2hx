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
		//Either String, Argument, or Parameter will be populated. Never more than one
		public readonly string StringOpt;
		public readonly ArgumentSyntax ArgumentOpt;
        public readonly ParameterSyntax ParameterOpt;

        public TransformedArgument(ParameterSyntax prm)
        {
            this.ParameterOpt = prm;
        }

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
