using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class TriviaProcessor
	{
		public static void ProcessNode(HaxeWriter writer, SyntaxNode node)
		{
			bool literalCode = false; //if we encounter a #if CS2HX, we set this to true, which indicates that the next DisabledTextTrivia should be written as pure code.   (TODO: Should we instead parse it into a syntax tree and transform from C#? We wouldn't have type info)

			foreach (var trivia in node.GetLeadingTrivia())
			{
				if (trivia.Kind == SyntaxKind.IfDirectiveTrivia)
				{
					var cond = trivia.ToString().Trim().RemoveFromStartOfString("#if ").Trim();
					if (cond == "CS2HX")
						literalCode = true;
				}
				else if (trivia.Kind == SyntaxKind.DisabledTextTrivia && literalCode)
				{
					writer.Write(trivia.ToString());
					literalCode = false;
				}
			}
		}

		/// <summary>
		/// Remove nodes that are to be omitted due to an #if !CS2HX
		/// </summary>
		/// <param name="tree"></param>
		/// <returns></returns>
		public static IEnumerable<SyntaxNode> DoNotWrite(SyntaxTree tree)
		{
			var skipCount = 0; //set to 1 if we encounter a #if !CS2HX directive (while it's 0).  Incremented for each #if that's started inside of that, and decremented for each #endif

			var ret = new List<SyntaxNode>();

			Action<SyntaxNode> recurse = null;
			recurse = node =>
				{

					foreach (var trivia in node.GetLeadingTrivia())
					{
						if (trivia.Kind == SyntaxKind.EndIfDirectiveTrivia && skipCount > 0)
							skipCount--;
						else if (trivia.Kind == SyntaxKind.IfDirectiveTrivia && skipCount > 0)
							skipCount++;
						else if (trivia.Kind == SyntaxKind.IfDirectiveTrivia && trivia.ToString().Trim().RemoveFromStartOfString("#if ").Trim() == "!CS2HX" && skipCount == 0)
							skipCount = 1;
					}

					if (skipCount > 0)
						ret.Add(node);
					
					foreach (var child in node.ChildNodes())
						recurse(child);
				};

			var root = tree.GetRoot();
			recurse(root);

			return ret;
		}
	}
}
