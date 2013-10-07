using System;
using System.Collections.Concurrent;
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
		private static ConcurrentDictionary<SyntaxTree, SemanticModel> _models = new ConcurrentDictionary<SyntaxTree, SemanticModel>();

		[ThreadStatic]
		public static TypeState Instance;

		public List<BaseTypeDeclarationSyntax> Partials;
		public string TypeName;
		public Compilation Compilation;

		public bool DerivesFromObject;
		public List<VariableDeclaratorSyntax> InstanceFieldsNeedingInitialization;
		public List<VariableDeclaratorSyntax> StaticFieldsNeedingInitialization;

		public SemanticModel GetModel(SyntaxNode node)
		{
			var tree = node.SyntaxTree;

			SemanticModel ret;
			if (_models.TryGetValue(tree, out ret))
				return ret;

			ret = Compilation.GetSemanticModel(tree);

			_models.TryAdd(tree, ret);

			return ret;
		}
	}
}
