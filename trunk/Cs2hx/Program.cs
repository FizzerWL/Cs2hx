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

		public static HashSet<string> StaticConstructors = new HashSet<string>();
		public static ConcurrentDictionary<SyntaxNode, object> DoNotWrite = new ConcurrentDictionary<SyntaxNode, object>();
		public static ConcurrentDictionary<Symbol, object> RefOutSymbols = new ConcurrentDictionary<Symbol, object>();
		public static string OutDir;

		public static void Go(Compilation compilation, string outDir, IEnumerable<string> extraTranslation)
		{
			Compilation = compilation;
			OutDir = outDir;

			var sw = Stopwatch.StartNew();

			//Test if it builds so we can fail early if we don't.  This isn't required for anything else to work.
			var buildResult = compilation.Emit(new MemoryStream());
			if (buildResult.Success == false)
				throw new Exception("Build failed. " + buildResult.Diagnostics.Count() + " errors: " + string.Join("", buildResult.Diagnostics.Select(o => "\n  " + o.ToString())));
			Console.WriteLine("Build succeeded in " + sw.Elapsed);
			sw.Restart();

			TranslationManager.Init(extraTranslation);

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			var allTypes = compilation.SyntaxTrees
				.SelectMany(o => o.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
				.Select(o => new { Syntax = o, Symbol = GetModel(o).GetDeclaredSymbol(o), TypeName = WriteType.TypeName(GetModel(o).GetDeclaredSymbol(o)) })
				.GroupBy(o => o.Symbol.ContainingNamespace.FullName() + "." + o.TypeName)
				.ToList();



			Utility.Parallel(compilation.SyntaxTrees.ToList(), tree =>
				{
					foreach (var n in TriviaProcessor.DoNotWrite(tree))
						DoNotWrite.TryAdd(n, null);
				});

			Console.WriteLine("Parsed in " + sw.Elapsed);
			sw.Restart();

			compilation.SyntaxTrees.SelectMany(o => o.GetRoot().DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>())
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

			Console.WriteLine("Haxe written out in " + sw.Elapsed);
		}

    }

}
