using System;
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
	static class WriteTryStatement
	{
		public static void Go(HaxeWriter writer, TryStatementSyntax tryStatement)
		{
			if (tryStatement.Finally != null)
				throw new Exception("Finally blocks are not supported in haxe. " + Utility.Descriptor(tryStatement.Finally));

			writer.WriteLine("try");
			Core.Write(writer, tryStatement.Block);

			foreach (var catchClause in tryStatement.Catches)
			{
				if (Program.DoNotWrite.ContainsKey(catchClause))
					continue;

				writer.WriteIndent();
				writer.Write("catch (");

				if (catchClause.Declaration == null)
					writer.Write("__ex:Dynamic");
				else
				{
                    var varName = catchClause.Declaration.Identifier.ValueText;

                    if (string.IsNullOrWhiteSpace(varName))
                        varName = "__ex";


                    writer.Write(varName);

                    var type = TypeProcessor.ConvertTypeWithColon(catchClause.Declaration.Type);

                    if (type == ":system.Exception")
                    {
                        //when the C# code catches Exception, we assume they want to catch everything, and in haxe we do that by catching Dynamic.  In this case, we also want to ensure the C# code never treats the exception as an Exception, since it might not be an actual Exception type in haxe.  C# code should be changed to only call .ToString() on it.
                        writer.Write(":Dynamic");

                        Func<IdentifierNameSyntax, bool> isOkUseOfException = node =>
                        {
                            //Calling .ToString() is OK on exceptions
                            if (node.Parent is MemberAccessExpressionSyntax
                                && node.Parent.Parent is InvocationExpressionSyntax
                                && node.Parent.Parent.As<InvocationExpressionSyntax>().Expression is MemberAccessExpressionSyntax
                                && node.Parent.Parent.As<InvocationExpressionSyntax>().Expression.As<MemberAccessExpressionSyntax>().Name.Identifier.ValueText == "ToString")
                                return true;

                            //Using them as concatenation in strings is OK
                            if (node.Parent is BinaryExpressionSyntax && node.Parent.As<BinaryExpressionSyntax>().OperatorToken.Kind() == SyntaxKind.PlusToken)
                                return true; //we only check that it's a PlusToken, which could be addition or string concatenation, but C# doesn't allow adding exceptions so it's not necessary to check further

                            var typeInfo = Program.GetModel(node).GetTypeInfo(node);
                            if (typeInfo.ConvertedType.SpecialType == SpecialType.System_Object)
                                return true; //OK to use it as an object, since that becomes Dynamic in haxe

                            return false;
                        };

                        var usesException = catchClause.Block.DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Where(o => o.Identifier.ValueText == varName)
                            .Where(o => !isOkUseOfException(o))
                            .ToList();

                        if (usesException.Count > 0)
                            throw new Exception("When catching an Exception, you cannot use the object as an Exception object, since the destination platform supports throwing things that don't derive from Exception.  Instead, call .ToString() on it if you need details of it.  " + string.Join(",  ", usesException.Select(Utility.Descriptor)));

                    }
                    else
                        writer.Write(type);
				}
				writer.Write(")\r\n");
				Core.Write(writer, catchClause.Block);
			}

		}
	}
}
