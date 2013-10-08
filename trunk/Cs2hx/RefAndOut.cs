using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
	static class RefAndOut
	{
		public static void ScopeBeginning(HaxeWriter writer, ExpressionSyntax scope)
		{
			foreach (var invoke in scope.DescendantNodes().OfType<InvocationExpressionSyntax>())
			{

			}
		}
	}
}
