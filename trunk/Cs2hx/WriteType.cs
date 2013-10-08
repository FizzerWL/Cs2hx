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

				TypeState.Instance.DerivesFromObject = bases.Count == interfaces.Count;

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

				if (first is EnumDeclarationSyntax)
					WriteEnumBody.Go(writer, TypeState.Instance.Partials.Cast<EnumDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !Program.DoNotWrite.ContainsKey(o)));
				else
				{
					var allChildren = partials.Cast<TypeDeclarationSyntax>().SelectMany(o => o.Members).Where(o => !Program.DoNotWrite.ContainsKey(o)).ToList();

					var fields = allChildren.OfType<FieldDeclarationSyntax>().Where(o => !Program.DoNotWrite.ContainsKey(o));
					var staticFields = fields.Where(o => o.Modifiers.Any(m => m.ValueText == "static"));
					TypeState.Instance.StaticFieldsNeedingInitialization = staticFields
						.SelectMany(o => o.Declaration.Variables)
						.Where(o => 
							(o.Initializer != null && !WriteField.IsConst(o.Parent.Parent.As<FieldDeclarationSyntax>().Modifiers, o.Initializer))
							||
							(o.Initializer == null && TypeProcessor.ValueToReference(o.Parent.As<VariableDeclarationSyntax>().Type)))
						.ToList();

					TypeState.Instance.InstanceFieldsNeedingInitialization = 
						fields
						.Except(staticFields)
						.SelectMany(o => o.Declaration.Variables)
						.Where(o => 
							(o.Initializer != null && !WriteField.IsConst(o.Parent.Parent.As<FieldDeclarationSyntax>().Modifiers, o.Initializer))
							||
							(o.Initializer == null && TypeProcessor.ValueToReference(o.Parent.As<VariableDeclarationSyntax>().Type)))
						.ToList();


					foreach (var partial in partials)
					{
						foreach (var member in partial.As<TypeDeclarationSyntax>().Members)
						{
							if (member is ClassDeclarationSyntax)
								throw new Exception("Subclasses are not supported " + Utility.Descriptor(member));

							Core.Write(writer, member);
						}
					}

					if (first.Kind != SyntaxKind.InterfaceDeclaration)
					{
						var ctors = allChildren.OfType<ConstructorDeclarationSyntax>().ToList();


						if (ctors.None(o => o.Modifiers.Any(SyntaxKind.StaticKeyword)))
							WriteConstructor.WriteStaticConstructor(writer, null);
						if (ctors.None(o => !o.Modifiers.Any(SyntaxKind.StaticKeyword)))
							WriteConstructor.WriteInstanceConstructor(writer, null);
						
					}
				}

				writer.WriteCloseBrace();
			}
		}
	}
}
