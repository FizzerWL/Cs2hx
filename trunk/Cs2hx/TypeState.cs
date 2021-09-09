using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	class TypeState
	{
		[ThreadStatic]
		public static TypeState Instance;

		public List<SyntaxAndSymbol> Partials;
		public string TypeName;

		public bool DerivesFromObject;
		public List<MemberDeclarationSyntax> AllMembers;


		public class SyntaxAndSymbol
		{
			public BaseTypeDeclarationSyntax Syntax;
			public INamedTypeSymbol Symbol;
		}
	}
}
