using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class WriteType
	{

		public static void Go(string outDir)
		{
			var partials = TypeState.Instance.Partials;
			var first = partials.First();

			var typeNamespace = first.Symbol.ContainingNamespace.FullName().ToLower();

			var dir = Path.Combine(outDir, typeNamespace.Replace(".", Path.DirectorySeparatorChar.ToString()));
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (var writer = new HaxeWriter(Path.Combine(dir, TypeState.Instance.TypeName + ".hx")))
			{
				var bases = partials
					.Select(o => o.Syntax.BaseList)
					.Where(o => o != null)
					.SelectMany(o => o.Types)
					.Select(o => (TypeSymbol)Program.GetModel(o).GetTypeInfo(o).ConvertedType)
					.Distinct()
					.ToList();

				var interfaces = bases.Where(o => o.TypeKind == TypeKind.Interface).ToList();

				TypeState.Instance.DerivesFromObject = bases.Count == interfaces.Count;

				writer.WriteLine("package " + typeNamespace + @";");

				WriteImports.Go(writer);

				switch (first.Syntax.Kind)
				{
					case SyntaxKind.ClassDeclaration:
					case SyntaxKind.StructDeclaration:
					case SyntaxKind.EnumDeclaration:
						writer.Write("class ");
						break;
					case SyntaxKind.InterfaceDeclaration:
						writer.Write("interface ");
						break;
					default:
						throw new Exception(first.Syntax.Kind.ToString());
				}
				
				writer.Write(TypeState.Instance.TypeName);



				if (first.Syntax is TypeDeclarationSyntax)
				{
					//Look for generic arguments 
					var genericArgs = partials
						.Select(o => o.Syntax)
						.Cast<TypeDeclarationSyntax>()
						.Where(o => o.TypeParameterList != null)
						.SelectMany(o => o.TypeParameterList.Parameters)
						.ToList();

					if (genericArgs.Count > 0)
					{
						writer.Write("<");
						writer.Write(string.Join(", ", genericArgs.Select(o => o.Identifier.ValueText)));
						writer.Write(">");
					}

					foreach (var baseType in bases)
					{
						writer.Write(" ");

						if (baseType.TypeKind == TypeKind.Interface)
						{
							writer.Write("implements ");
							writer.Write(TypeProcessor.ConvertType(baseType));
						}
						else
						{
							writer.Write("extends ");
							writer.Write(TypeProcessor.ConvertType(baseType));
						}
					}
				}

				writer.Write("\r\n");

				writer.WriteOpenBrace();

				if (first.Syntax is EnumDeclarationSyntax)
					WriteEnumBody.Go(writer, TypeState.Instance.Partials.Select(o => o.Syntax).Cast<EnumDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !Program.DoNotWrite.ContainsKey(o)));
				else
				{
					TypeState.Instance.AllMembers = partials.Select(o => o.Syntax).Cast<TypeDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !Program.DoNotWrite.ContainsKey(o)).ToList();

					foreach (var partial in partials)
					{
						foreach (var member in partial.Syntax.As<TypeDeclarationSyntax>().Members)
						{
							if (!(member is ClassDeclarationSyntax) && !(member is EnumDeclarationSyntax))
								Core.Write(writer, member);
						}
					}

					if (first.Syntax.Kind != SyntaxKind.InterfaceDeclaration)
					{
						//Normally constructors will be written as we traverse the tree.  However, if there are no constructors, we must manually write them out since there are cases where we need a constructor in haxe while C# had none.
						var ctors = TypeState.Instance.AllMembers.OfType<ConstructorDeclarationSyntax>().ToList();

						if (ctors.None(o => o.Modifiers.Any(SyntaxKind.StaticKeyword)))
							WriteConstructor.WriteStaticConstructor(writer, null);
						if (ctors.None(o => !o.Modifiers.Any(SyntaxKind.StaticKeyword)))
							WriteConstructor.WriteInstanceConstructor(writer, null);
						
					}
				}

				writer.WriteCloseBrace();
			}
		}

		public static string TypeName(NamedTypeSymbol type)
		{
			var sb = new StringBuilder(type.Name);

			while (type.ContainingType != null)
			{
				type = type.ContainingType;
				sb.Insert(0, type.Name + "_");
			}

			return sb.ToString();
		}
	}
}
