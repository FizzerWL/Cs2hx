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
        static internal HashSet<string> StaticConstructors = new HashSet<string>();

		public static void Go(Compilation compilation, string outDir, IEnumerable<string> extraTranslation)
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

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			var allNamespaces = compilation.SyntaxTrees.SelectMany(o => o.GetRoot().ChildNodes().OfType<NamespaceDeclarationSyntax>())
				.GroupBy(o => o.Name.ToString())
				.Select(o => new { NamespaceName = o.Key, Namespaces = o })
				.ToList();

			var typesGroupedByNamespace = allNamespaces.Select(o =>
				o.Namespaces.SelectMany(n => n.Members.OfType<BaseTypeDeclarationSyntax>())
					.GroupBy(t => t.Identifier.ValueText)
					.Select(t => new { TypeName = t.Key, Partials = t }))
					.ToList();

			var allTypes = typesGroupedByNamespace.SelectMany(o => o).SelectMany(o => o.Partials);


			foreach (var ns in typesGroupedByNamespace)
				foreach (var type in ns)
				{
					TypeState.Instance = new TypeState();
					TypeState.Instance.Compilation = compilation;
					TypeState.Instance.TypeName = type.TypeName;
					TypeState.Instance.DoNotWrite = type.Partials.Select(o => o.SyntaxTree).Distinct().SelectMany(TriviaProcessor.DoNotWrite).ToHashSet(false);
					TypeState.Instance.Partials = type.Partials.Where(o => !TypeState.Instance.DoNotWrite.Contains(o)).ToList();
					TypeState.Instance.GetTypesInNamespace = t => allNamespaces.Where(o => o.NamespaceName == t).SelectMany(o => o.Namespaces).SelectMany(o => o.Members.OfType<TypeDeclarationSyntax>());

					if (TypeState.Instance.Partials.Count > 0)
						WriteType.Go(outDir);
				}


			Console.WriteLine("Done in " + sw.Elapsed);
		}

    }

}
