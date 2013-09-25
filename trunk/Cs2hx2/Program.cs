using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System.IO;
using Roslyn.Compilers.Common;
using System.Diagnostics;

namespace Cs2hx
{
    public static class Program
    {
        static internal List<XDocument> TranslationDocs;
        static internal string OutDir;
        //static internal List<MethodDeclarationSyntax> ConvertingExtensionMethods;
        //static internal HashSet<string> EnumNames;
        //static internal Dictionary<string, IEnumerable<DelegateDeclarationSyntax>> Delegates;
        static internal HashSet<string> StaticConstructors = new HashSet<string>();

		public static void Go(CommonCompilation compilation, string outDir, IEnumerable<string> extraTranslation)
		{
			var sw = Stopwatch.StartNew();

			//Test if it builds so we can fail early if we don't.  This isn't required for anything else to work.
			Console.WriteLine("Building...");
			var buildResult = compilation.Emit(new MemoryStream());
			if (buildResult.Success == false)
				throw new Exception("Build failed. " + buildResult.Diagnostics.Count() + " errors: " + string.Join("", buildResult.Diagnostics.Select(o => "\n  " + o.ToString())));
			Console.WriteLine("Build succeeded in " + sw.Elapsed);
			sw.Restart();


			TranslationDocs = Translations.Translation.BuildTranslationDocs(extraTranslation);

			OutDir = outDir;
			if (!Directory.Exists(OutDir))
				Directory.CreateDirectory(outDir);

			var trees = compilation.SyntaxTrees.ToList();

			var allNamespaces = trees.SelectMany(o => o.GetRoot().ChildNodes().OfType<NamespaceDeclarationSyntax>())
				.GroupBy(o => o.Name.ToString())
				.Select(o => new { NamespaceName = o.Key, Namespaces = o })
				.ToList();

			var typesGroupedByNamespace = allNamespaces.Select(o =>
				o.Namespaces.SelectMany(n => n.Members.OfType<TypeDeclarationSyntax>())
					.GroupBy(t => t.Identifier.ValueText)
					.Select(t => new { TypeName = t.Key, Partials = t }))
					.ToList();

			var allTypes = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials);

			//ConvertingExtensionMethods = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials)
			//    .SelectMany(o => o.Members)
			//    .OfType<MethodDeclarationSyntax>()
			//    .Where(o => o.IsExtensionMethod)
			//    .ToList();


			//EnumNames = allTypes.Where(o => o.Type == ClassType.Enum).Select(o => o.Name).ToHashSet(true);

			//Delegates = allNamespaces.SelectMany(o => o.Namespaces).SelectMany(o => o.Children)
			//    .OfType<DelegateDeclaration>()
			//    .Concat(allTypes.SelectMany(o => o.Children).OfType<DelegateDeclaration>())
			//    .GroupBy(o => o.Name)
			//    .ToDictionary(o => o.Key, o => (IEnumerable<DelegateDeclaration>)o);

			foreach (var ns in typesGroupedByNamespace)
				foreach (var type in ns)
				{
					TypeState.Instance = new TypeState();
					TypeState.Instance.Compilation = compilation;
					TypeState.Instance.TypeName = type.TypeName;
					TypeState.Instance.Partials = type.Partials.ToList();
					TypeState.Instance.GetTypesInNamespace = t => allNamespaces.Where(o => o.NamespaceName == t).SelectMany(o => o.Namespaces).SelectMany(o => o.Members.OfType<TypeDeclarationSyntax>());
					GenerateType();
				}


			Console.WriteLine("Done in " + sw.Elapsed);
		}

        static private void GenerateType()
        {
			var first = TypeState.Instance.Partials.First();

            var typeNamespace = first.Parent.As<NamespaceDeclarationSyntax>();

            var dir = Path.Combine(OutDir, typeNamespace.Name.ToString().Replace(".", Path.DirectorySeparatorChar.ToString())).ToLower();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

			using (var writer = new HaxeWriter(Path.Combine(dir, TypeState.Instance.TypeName + ".hx")))
            {
                bool derivesFromObject = true; //TODO

				writer.WriteLine("package " + typeNamespace.Name.ToString().ToLower() + @";");

				WriteImports.Go(writer);

                writer.Write("class ");
				writer.Write(TypeState.Instance.TypeName);
                writer.Write("\r\n");

                writer.WriteOpenBrace();

                var allChildren = TypeState.Instance.Partials.SelectMany(o => o.Members).ToList();

                var fields = allChildren.OfType<FieldDeclarationSyntax>();
                var staticFields = fields.Where(o => o.Modifiers.Any(m => m.ValueText == "static"));
                var staticFieldsNeedingInitialization = staticFields.SelectMany(o => o.Declaration.Variables).Where(o => o.Initializer != null);
                var instanceFieldsNeedingInitialization = fields.Except(staticFields).SelectMany(o => o.Declaration.Variables).Where(o => o.Initializer != null);
                    
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

                writer.WriteCloseBrace();
            }
        }
    }

}
