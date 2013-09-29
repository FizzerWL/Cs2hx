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

			var typeNamespace = first.Parent.As<NamespaceDeclarationSyntax>();

			var dir = Path.Combine(outDir, typeNamespace.Name.ToString().Replace(".", Path.DirectorySeparatorChar.ToString())).ToLower();
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using (var writer = new HaxeWriter(Path.Combine(dir, TypeState.Instance.TypeName + ".hx")))
			{
				var bases = partials
					.Select(o => o.BaseList)
					.Where(o => o != null)
					.SelectMany(o => o.Types)
					.Select(o => (TypeSymbol)TypeState.Instance.GetModel(o).GetTypeInfo(o).ConvertedType)
					.Distinct()
					.ToList();

				var interfaces = bases.Where(o => o.TypeKind == TypeKind.Interface).ToList();

				bool derivesFromObject = bases.Count == interfaces.Count;

				writer.WriteLine("package " + typeNamespace.Name.ToString().ToLower() + @";");

				WriteImports.Go(writer);

				switch (first.Kind)
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
						throw new Exception(first.Kind.ToString());
				}
				
				writer.Write(TypeState.Instance.TypeName);



				if (first is TypeDeclarationSyntax)
				{
					//Look for generic arguments 
					var genericArgs = partials
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


					writer.Write(" ");

					var firstExtends = true;
					foreach (var baseType in bases)
					{
						if (firstExtends)
							firstExtends = false;
						else
							writer.Write(", ");

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

				if (first is TypeDeclarationSyntax)
				{

					var allChildren = TypeState.Instance.Partials.Cast<TypeDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !TypeState.Instance.DoNotWrite.Contains(o)).ToList();

					var fields = allChildren.OfType<FieldDeclarationSyntax>().Where(o => !TypeState.Instance.DoNotWrite.Contains(o));
					var staticFields = fields.Where(o => o.Modifiers.Any(m => m.ValueText == "static"));
					var staticFieldsNeedingInitialization = staticFields.SelectMany(o => o.Declaration.Variables).Where(o => o.Initializer != null &&  !WriteFields.IsConst(o.Parent.Parent.As<FieldDeclarationSyntax>().Modifiers, o.Initializer));
					var instanceFieldsNeedingInitialization = fields.Except(staticFields).SelectMany(o => o.Declaration.Variables).Where(o => o.Initializer != null && !WriteFields.IsConst(o.Parent.Parent.As<FieldDeclarationSyntax>().Modifiers, o.Initializer));

					WriteFields.Go(writer, allChildren.OfType<FieldDeclarationSyntax>());
					writer.WriteLine();
					WriteProperties.Go(writer, allChildren.OfType<PropertyDeclarationSyntax>());
					writer.WriteLine();
					WriteMethods.Go(writer, allChildren.OfType<MethodDeclarationSyntax>(), derivesFromObject);

					if (first.Kind != SyntaxKind.InterfaceDeclaration)
					{
						writer.WriteLine();
						WriteConstructors.Go(writer, allChildren.OfType<ConstructorDeclarationSyntax>(), derivesFromObject, instanceFieldsNeedingInitialization, staticFieldsNeedingInitialization);
					}
				}
				else
				{
					WriteEnumBody.Go(writer, TypeState.Instance.Partials.Cast<EnumDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !TypeState.Instance.DoNotWrite.Contains(o)));
				}

				writer.WriteCloseBrace();
			}
		}
	}
}
