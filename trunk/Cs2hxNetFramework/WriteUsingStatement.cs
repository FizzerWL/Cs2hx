using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cs2hx
{
	static class WriteUsingStatement
	{
        static Dictionary<MethodDeclarationSyntax, int> _identities = new Dictionary<MethodDeclarationSyntax, int>();

		public static void Go(HaxeWriter writer, UsingStatementSyntax usingStatement)
		{
			if (usingStatement.DescendantNodes().OfType<ReturnStatementSyntax>().Any())
				throw new Exception("CS2HX does not support returning from within a using block. " + Utility.Descriptor(usingStatement));

			var expression = usingStatement.Expression;
			//if (expression is ExpressionStatement)
			//	expression = expression.As<ExpressionStatement>().Expression;

			//Generate a resource to identify this using block.  If it's a local variable, we'll use that.
			var resource = Utility.TryGetIdentifier(expression);
            if (resource == null)
            {
                var parent = expression.Parent;
                while (!(parent is MethodDeclarationSyntax))
                    parent = parent.Parent;
                var containingMethod = (MethodDeclarationSyntax)parent;
                if (_identities == null)
                    throw new Exception("_identities is null");
                if (containingMethod == null)
                    throw new Exception("containingMethod is null");
                var id = _identities.ValueOrZero(containingMethod);
                _identities.AddTo(containingMethod, 1);
                
                resource = "__" + id + "_using";

                writer.WriteIndent();
                writer.Write("var " + resource + " = ");
                Core.Write(writer, expression);
                writer.WriteLine(";");
            }

            writer.WriteLine("var __" + resource + "_usingDisposed:Bool = false;");

            writer.WriteLine("try");
			writer.WriteOpenBrace();

			if (usingStatement.Statement is BlockSyntax)
				foreach (var s in usingStatement.Statement.As<BlockSyntax>().Statements)
					Core.Write(writer, s);
			else
				Core.Write(writer, usingStatement.Statement);

			writer.WriteLine("__" + resource + "_usingDisposed = true;");
			writer.WriteLine(resource + ".Dispose();");
			writer.WriteCloseBrace();

			writer.WriteLine("catch (__catch_" + resource + ":Dynamic)");
			writer.WriteOpenBrace();
			writer.WriteLine("if (!__" + resource + "_usingDisposed)");
			writer.WriteLine("    " + resource + ".Dispose();");
			writer.WriteLine("throw __catch_" + resource + ";");
			writer.WriteCloseBrace();
		}
	}
}
