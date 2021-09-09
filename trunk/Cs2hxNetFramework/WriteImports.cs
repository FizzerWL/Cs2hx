using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cs2hx.Translations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	public static class WriteImports
	{

		static public string StandardImports = 
@"using StringTools;
import system.*;
import anonymoustypes.*;
";
		

		public static void Go(HaxeWriter writer)
		{
			writer.WriteLine(StandardImports);
		}
	}
}
