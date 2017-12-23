using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cs2hx.Translations;

namespace Cs2hx
{
    public static class Program
    {
		private static ConcurrentDictionary<SyntaxTree, SemanticModel> _models = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
		private static Compilation Compilation;
		
		public static SemanticModel GetModel(SyntaxNode node)
		{
			var tree = node.SyntaxTree;

			SemanticModel ret;
			if (_models.TryGetValue(tree, out ret))
				return ret;

			ret = Compilation.GetSemanticModel(tree);

			_models.TryAdd(tree, ret);

			return ret;
		}

		public static ConcurrentDictionary<SyntaxNode, object> DoNotWrite = new ConcurrentDictionary<SyntaxNode, object>();
		public static ConcurrentDictionary<ISymbol, object> RefOutSymbols = new ConcurrentDictionary<ISymbol, object>();
		public static string OutDir;

		public static void Go(Compilation compilation, string outDir, IEnumerable<string> extraTranslation)
		{
            TranslationManager.Init(extraTranslation);

            Compilation = compilation.AddReferences(TranslationManager.References.Select(o => MetadataReference.CreateFromFile(o)));
            OutDir = outDir;

            Utility.Parallel(new Action[] { Build, Generate }, a => a());
		}

		private static void Build()
		{
			Console.WriteLine("Building...");
			var sw = Stopwatch.StartNew();

            //Test if it builds so we can fail early if we don't.  This isn't required for anything else to work.
            var buildResult = Compilation.Emit(new MemoryStream());
			if (buildResult.Success == false)
				throw new Exception("Build failed. " + buildResult.Diagnostics.Count() + " errors: " + string.Join("", buildResult.Diagnostics.Select(o => "\n  " + o.ToString())));
			Console.WriteLine("Built in " + sw.Elapsed);
		}

		private static void Generate()
		{
			Console.WriteLine("Parsing...");
			var sw = Stopwatch.StartNew();


			if (!Directory.Exists(OutDir))
				Directory.CreateDirectory(OutDir);

			var allTypes = Compilation.SyntaxTrees
				.SelectMany(o => o.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
				.Select(o => new { Syntax = o, Symbol = GetModel(o).GetDeclaredSymbol(o), TypeName = WriteType.TypeName(GetModel(o).GetDeclaredSymbol(o)) })
				.GroupBy(o => o.Symbol.ContainingNamespace.FullName() + "." + o.TypeName)
				.ToList();

			Utility.Parallel(Compilation.SyntaxTrees.ToList(), tree =>
				{
					foreach (var n in TriviaProcessor.DoNotWrite(tree))
						DoNotWrite.TryAdd(n, null);
				});

			Console.WriteLine("Parsed in " + sw.Elapsed + ". Writing out haxe...");
			sw.Restart();

			Compilation.SyntaxTrees.SelectMany(o => o.GetRoot().DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>())
				.Select(o => new { Syntax = o, Name = WriteAnonymousObjectCreationExpression.TypeName(o) })
				.GroupBy(o => o.Name)
				.Parallel(o => WriteAnonymousObjectCreationExpression.WriteAnonymousType(o.First().Syntax));


			allTypes.Parallel(type =>
				{
					TypeState.Instance = new TypeState();
					TypeState.Instance.TypeName = type.First().TypeName;
					TypeState.Instance.Partials = type.Select(o => new TypeState.SyntaxAndSymbol { Symbol = o.Symbol, Syntax = o.Syntax })
						.Where(o => !DoNotWrite.ContainsKey(o.Syntax))
						.ToList();

					if (TypeState.Instance.Partials.Count > 0)
						WriteType.Go();
				});

			WriteConstructor.WriteConstructorsHelper(allTypes.SelectMany(o => o).Where(o => !DoNotWrite.ContainsKey(o.Syntax)).Select(o => o.Symbol));

			Console.WriteLine("Haxe written out in " + sw.Elapsed);
		}

    }

}
