using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	class TypeState
	{
		[ThreadStatic]
		public static TypeState Instance;

		public List<TypeDeclarationSyntax> Partials;
		public Func<string, IEnumerable<TypeDeclarationSyntax>> GetTypesInNamespace;
		public string TypeName;
		public CommonCompilation Compilation;
		public HashSet<SyntaxNode> DoNotWrite; //these nodes should not be written. They're excluded by being wrapped by !CS2HX

		private Dictionary<SyntaxTree, ISemanticModel> _models = new Dictionary<SyntaxTree, ISemanticModel>();

		public ISemanticModel GetModel(SyntaxNode node)
		{
			var tree = node.SyntaxTree;

			ISemanticModel ret;
			if (_models.TryGetValue(tree, out ret))
				return ret;

			ret = Compilation.GetSemanticModel(tree);

			_models.Add(tree, ret);

			return ret;
		}
	}
}
