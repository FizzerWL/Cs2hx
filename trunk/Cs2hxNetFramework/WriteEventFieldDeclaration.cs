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
	/// <summary>
	/// Events are currently not supported.  This project contains some code that attempts to map events to the CsEvent type, and it could work when building for Flash. Unfortunately, it does not work with other targets, so it's left out for consistency.
	/// </summary>
	static class WriteEventFieldDeclaration
	{
		public static void Go(HaxeWriter writer, EventFieldDeclarationSyntax node)
		{
			throw new Exception("Events are not supported " + Utility.Descriptor(node)); 
			/*
			foreach (var declaration in node.Declaration.Variables)
			{

				writer.WriteIndent();
				WriteField.WriteFieldModifiers(writer, node.Modifiers);
				writer.Write("var ");
				writer.Write(declaration.Identifier.ValueText);
				writer.Write(":CsEvent<");
				writer.Write(TypeProcessor.ConvertType(node.Declaration.Type));
				writer.Write(">;\r\n");
			}*/
		}
	}
}
