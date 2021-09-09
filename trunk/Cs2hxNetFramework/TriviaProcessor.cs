using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	static class TriviaProcessor
	{
		static ConcurrentDictionary<SyntaxTrivia, object> _triviaProcessed = new ConcurrentDictionary<SyntaxTrivia, object>();
		public static void ProcessNode(HaxeWriter writer, SyntaxNode node)
		{
			ProcessTrivias(writer, node.GetLeadingTrivia());
		}

		public static void ProcessTrivias(HaxeWriter writer, IEnumerable<SyntaxTrivia> trivias)
		{
			bool literalCode = false; //if we encounter a #if CS2HX, we set this to true, which indicates that the next DisabledTextTrivia should be written as pure code.   

			foreach (var trivia in trivias)
			{
				if (_triviaProcessed.TryAdd(trivia, null)) //ensure we don't look at the same trivia multiple times
				{

					if (trivia.Kind() == SyntaxKind.IfDirectiveTrivia)
					{
						if (GetConditions(trivia, "#if ").Contains("CS2HX"))
							literalCode = true;
					}
					else if (trivia.Kind() == SyntaxKind.DisabledTextTrivia && literalCode)
					{
						writer.Write(trivia.ToString());
						literalCode = false;
					}
				}
			}
		}

		private static string[] GetConditions(SyntaxTrivia trivia, string lineStart)
		{
			var str = trivia.ToString().Trim().RemoveFromStartOfString("#if ").Trim();

			int i = str.IndexOf("//");
			if (i != -1)
				str = str.Substring(0, i).Trim();

			i = str.IndexOf("/*");
			if (i != -1)
				str = str.Substring(0, i).Trim();

			return str.Split("|& ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// Remove nodes that are to be omitted due to an #if !CS2HX or the else of an #if CS2HX
		/// </summary>
		/// <param name="tree"></param>
		/// <returns></returns>
		public static IEnumerable<SyntaxNode> DoNotWrite(SyntaxTree tree)
		{
			var triviaProcessed = new ConcurrentDictionary<SyntaxTrivia, object>();

			var skipCount = 0; //set to 1 if we encounter a #if !CS2HX directive (while it's 0).  Incremented for each #if that's started inside of that, and decremented for each #endif
			var elseCount = 0; //set to 1 if we encounter an #if CS2HX directive (while it's 0).  Incremented for each #if that's started inside of that, and decremented for each #endif

			var ret = new List<SyntaxNode>();

			Action<SyntaxNodeOrToken> recurse = null;
			recurse = node =>
				{
					Action<SyntaxTrivia> doTrivia = trivia =>
						{
							if (!triviaProcessed.TryAdd(trivia, null))
								return;

							if (trivia.Kind() == SyntaxKind.EndIfDirectiveTrivia)
							{
								if (skipCount > 0)
									skipCount--;
								if (elseCount > 0)
									elseCount--;
							}
							else if (trivia.Kind() == SyntaxKind.IfDirectiveTrivia)
							{
								if (skipCount > 0)
									skipCount++;
								if (elseCount > 0)
									elseCount++;

								var cond = GetConditions(trivia, "#if ");

								if (cond.Contains("!CS2HX") && skipCount == 0)
									skipCount = 1;
								else if (cond.Contains("CS2HX") && elseCount == 0)
									elseCount = 1;

							}
							else if (trivia.Kind() == SyntaxKind.ElseDirectiveTrivia)
							{
								if (elseCount == 1)
								{
									skipCount = 1;
									elseCount = 0;
								}
							}
						};

					foreach (var trivia in node.GetLeadingTrivia())
						doTrivia(trivia);

					if (skipCount > 0 && node.IsNode)
						ret.Add(node.AsNode());

					foreach (var child in node.ChildNodesAndTokens())
						recurse(child);

					foreach (var trivia in node.GetTrailingTrivia())
						doTrivia(trivia);

				};

			var root = tree.GetRoot();
			recurse(root);

			return ret;
		}
	}
}
