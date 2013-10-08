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
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	public static class WriteImports
	{
		static Dictionary<string, string> _allTypes;

		public static void Init(IEnumerable<KeyValuePair<string, string>> codeTypes)
		{
			//Start with all system files
			_allTypes = SystemImports.ToDictionary(o => o.SubstringAfterLast('.'), o => o);

			//Allow users to specify extra import statements in the xml file
			foreach (var extra in Translations.Translation.ExtraImports())
				_allTypes.Add(extra.SubstringAfterLast('.'), extra);

			//Add types we reference from haxe libraries
			_allTypes.Add("Bytes", "haxe.io.Bytes");

			foreach (var codeType in codeTypes)
				_allTypes.Add(codeType.Value, codeType.Key.Length == 0 ? codeType.Value : (codeType.Key.ToLower() + "." + codeType.Value));
		}

		#region Standard imports
		/// <summary>
		/// Anything that we need always available, even if nothing in user code references it.
		/// </summary>
		static public string StandardImports = @"using StringTools;
import system.Cs2Hx;
import system.Exception;";

		/// <summary>
		/// TODO: Calculate these by parsing our system haxe dir
		/// </summary>
		static private string[] _systemImports;


		static string[] SystemImports
		{
			get
			{
				if (_systemImports == null) //not threadsafe, but it's fine since it doesn't hurt if we load them twice
					_systemImports = LoadSystemImports(); 
				return _systemImports;
			}
		}

		private static string[] LoadSystemImports()
		{
			return Directory.GetFiles(GetSystemDir(), "*.hx", SearchOption.AllDirectories)
				.Select(GetTypeName)
				.Where(o => o != null)
				.ToArray();
		}

		private static string GetSystemDir()
		{
			var cfg = ConfigurationManager.AppSettings["PathToSystemDir"];

			if (cfg != null && Directory.Exists(cfg))
				return cfg;

			var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			cfg = Path.Combine(appDir, "system");
			if (Directory.Exists(cfg))
				return cfg;
			cfg = Path.Combine(appDir, "../system");
			if (Directory.Exists(cfg))
				return cfg;
			cfg = Path.Combine(appDir, "../../system");
			if (Directory.Exists(cfg))
				return cfg;
			cfg = Path.Combine(appDir, "../../../system");
			if (Directory.Exists(cfg))
				return cfg;

			throw new Exception("Could not find system dir");
		}
		
		private static string GetTypeName(string pathToHaxeFile)
		{
			var file = File.ReadAllText(pathToHaxeFile);

			var package = Regex.Match(file, @"^package (?<packageName>.*);", RegexOptions.Multiline);
			if (!package.Success)
				return null;

			var type = Regex.Match(file, @"^class (?<typeName>\w+)", RegexOptions.Multiline);
			if (!type.Success)
			{
				type = Regex.Match(file, @"^interface (?<typeName>\w+)", RegexOptions.Multiline);
				if (!type.Success)
					return null;
			}

			return package.Groups["packageName"].Value + "." + type.Groups["typeName"].Value;
		}

		#endregion


		public static void Go(HaxeWriter writer)
		{
			var partials = TypeState.Instance.Partials;

			//Standard ones are always present, since we can't easily determine if it should be filtered
			writer.WriteLine(StandardImports);

			var allNodes = partials.SelectMany(classType => DescendantNodes(classType.Syntax));
			var typesReferenced = allNodes.OfType<TypeSyntax>()
				.Select(o => TypeProcessor.TryConvertType(o))
				.Where(o => o != null)
				.Distinct()
				.SelectMany(SplitGenericTypes)
				//.Concat(typeObjects.Select(o => o.Type))
				.ToHashSet(false);

			//Lambda return types and argument types get printed out, but since Roslyn doesn't include them as TypeSyntax nodes the above statement won't see them.  Add them in here.
			Func<MethodSymbol, IEnumerable<TypeSymbol>> allTypes = method =>
				{
					var t = method.Parameters.ToList().Select(o => o.Type).Concat(method.TypeArguments.ToList());
					if (!method.ReturnsVoid)
						t = t.Concat(method.ReturnType);
					return t;
				};
			foreach (var name in allNodes.Where(o => o is ParenthesizedLambdaExpressionSyntax || o is SimpleLambdaExpressionSyntax)
				.Select(o => Program.GetModel(o).GetTypeInfo((ExpressionSyntax)o).ConvertedType.As<NamedTypeSymbol>().DelegateInvokeMethod.As<MethodSymbol>())
				.SelectMany(allTypes)
				.Select(TypeProcessor.ConvertType)
				.SelectMany(SplitGenericTypes))
				typesReferenced.Add(name);



			//Add in static references.  Any MemberAccess where the expression is a simple identifier means it's the root of a static call
			foreach (var symbol in allNodes.OfType<MemberAccessExpressionSyntax>()
				.Select(o => o.Expression)
				.OfType<IdentifierNameSyntax>()
				.Select(o => Program.GetModel(o).GetSymbolInfo(o).Symbol)
				.OfType<NamedTypeSymbol>()
				.Where(o => o.Kind == SymbolKind.NamedType))
				typesReferenced.Add(symbol.Name);


			//Add in extension methods.
			foreach (var symbol in allNodes.OfType<InvocationExpressionSyntax>()
				.Select(o => Program.GetModel(o).GetSymbolInfo(o).Symbol.As<MethodSymbol>().UnReduce())
				.Where(o => o.IsExtensionMethod))
				typesReferenced.Add(Translation.ExtensionName(symbol.ContainingType));

			foreach (var doNotWrite in Program.DoNotWriteTypeNames.Keys)
				typesReferenced.Remove(doNotWrite);

			//Don't import ourself
			typesReferenced.Remove(TypeState.Instance.TypeName);

			var imports = _allTypes.Where(o => typesReferenced.Contains(o.Key)).Select(o => o.Value);

			//Write the imports
			foreach (var import in imports.OrderBy(o => o))
				writer.WriteLine("import " + import + ";");


		}

		private static IEnumerable<SyntaxNode> DescendantNodes(SyntaxNode node)
		{
			foreach (var child in node.ChildNodes())
			{
				if (child is AttributeSyntax)
					continue;

				yield return child;

				foreach (var c2 in DescendantNodes(child))
					yield return c2;

			}
		}


		static string[] GenericTokens = new string[] { "->", "(", ")", "<", ">", " " };

		private static List<string> SplitGenericTypes(string typeString)
		{
			int readerIndex = 0;

			Func<char, bool> isLiteralChar = c => char.IsLetterOrDigit(c) || c == '_';

			Func<string> readToken = () =>
			{
				var sb = new StringBuilder();
				var first = typeString[readerIndex++];
				sb.Append(first.ToString());
				while (readerIndex < typeString.Length)
				{
					var c = typeString[readerIndex];

					if (isLiteralChar(c) != isLiteralChar(first))
						return sb.ToString().Trim();

					sb.Append(c.ToString());
					readerIndex++;
				}
				return sb.ToString().Trim();
			};

			var ret = new List<string>();

			while (readerIndex < typeString.Length)
			{
				var token = readToken();

				if (token.Length == 0 || token == "." || token == ",")
					continue;

				if (!GenericTokens.Contains(token))
					ret.Add(token);
			}

			return ret;
		}


	}
}
