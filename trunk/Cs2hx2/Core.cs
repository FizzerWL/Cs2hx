using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace Cs2hx
{
    static class Core
    {
        public static void Write(HaxeWriter writer, SyntaxNode node)
        {
			if (node is ExpressionStatementSyntax)
				WriteStatement(writer, node.As<ExpressionStatementSyntax>());
			else if (node is LocalDeclarationStatementSyntax)
				WriteLocalDeclaration.Go(writer, node.As<LocalDeclarationStatementSyntax>());
			else if (node is BlockSyntax)
				WriteBlock(writer, node.As<BlockSyntax>());
			else if (node is InvocationExpressionSyntax)
				WriteInvocationExpression.Go(writer, node.As<InvocationExpressionSyntax>());
			else if (node is IdentifierNameSyntax || node is LiteralExpressionSyntax)
				writer.Write(node.ToString());
			else if (node is ImplicitArrayCreationExpressionSyntax)
				WriteArrayCreationExpression.Go(writer, node.As<ImplicitArrayCreationExpressionSyntax>());
			else if (node is ArrayCreationExpressionSyntax)
				WriteArrayCreationExpression.Go(writer, node.As<ArrayCreationExpressionSyntax>());
			else if (node is MemberAccessExpressionSyntax)
				WriteMemberAccessExpression.Go(writer, node.As<MemberAccessExpressionSyntax>());
			else if (node is ParenthesizedLambdaExpressionSyntax)
				WriteLambdaExpression.Go(writer, node.As<ParenthesizedLambdaExpressionSyntax>());
			else if (node is SimpleLambdaExpressionSyntax)
				WriteLambdaExpression.Go(writer, node.As<SimpleLambdaExpressionSyntax>());
			else if (node is BinaryExpressionSyntax)
				WriteOperatorExpression.Go(writer, node.As<BinaryExpressionSyntax>());
			else if (node is ReturnStatementSyntax)
				WriteReturnStatement.Go(writer, node.As<ReturnStatementSyntax>());
			else
				throw new NotImplementedException();
        }

		public static void WriteStatement(HaxeWriter writer, ExpressionStatementSyntax statement)
        {
            writer.WriteIndent();
			Write(writer, statement.Expression);
            writer.Write(";\r\n");
        }

		public static void WriteBlock(HaxeWriter writer, BlockSyntax block)
		{
			writer.WriteOpenBrace();
			foreach (var statement in block.Statements)
				Write(writer, statement);
			writer.WriteCloseBrace();
		}

        
    }
}
