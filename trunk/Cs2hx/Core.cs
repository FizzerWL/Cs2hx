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
			TriviaProcessor.ProcessNode(writer, node);

			if (Program.DoNotWrite.ContainsKey(node))
				return;

			if (node is MethodDeclarationSyntax)
				WriteMethod.Go(writer, node.As<MethodDeclarationSyntax>());
			else if (node is PropertyDeclarationSyntax)
				WriteProperty.Go(writer, node.As<PropertyDeclarationSyntax>());
			else if (node is FieldDeclarationSyntax)
				WriteField.Go(writer, node.As<FieldDeclarationSyntax>());
			else if (node is ConstructorDeclarationSyntax)
				WriteConstructor.Go(writer, node.As<ConstructorDeclarationSyntax>());
			else if (node is ExpressionStatementSyntax)
				WriteStatement(writer, node.As<ExpressionStatementSyntax>());
			else if (node is LocalDeclarationStatementSyntax)
				WriteLocalDeclaration.Go(writer, node.As<LocalDeclarationStatementSyntax>());
			else if (node is BlockSyntax)
				WriteBlock(writer, node.As<BlockSyntax>());
			else if (node is InvocationExpressionSyntax)
				WriteInvocationExpression.Go(writer, node.As<InvocationExpressionSyntax>());
			else if (node is LiteralExpressionSyntax)
				WriteLiteralExpression.Go(writer, node.As<LiteralExpressionSyntax>());
			else if (node is IdentifierNameSyntax)
				WriteIdentifierName.Go(writer, node.As<IdentifierNameSyntax>());
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
			else if (node is ReturnStatementSyntax)
				WriteReturnStatement.Go(writer, node.As<ReturnStatementSyntax>());
			else if (node is ObjectCreationExpressionSyntax)
				WriteObjectCreationExpression.Go(writer, node.As<ObjectCreationExpressionSyntax>());
			else if (node is ElementAccessExpressionSyntax)
				WriteElementAccessExpression.Go(writer, node.As<ElementAccessExpressionSyntax>());
			else if (node is ForEachStatementSyntax)
				WriteForEachStatement.Go(writer, node.As<ForEachStatementSyntax>());
			else if (node is IfStatementSyntax)
				WriteIfStatement.Go(writer, node.As<IfStatementSyntax>());
			else if (node is BinaryExpressionSyntax)
				WriteBinaryExpression.Go(writer, node.As<BinaryExpressionSyntax>());
			else if (node is ConditionalExpressionSyntax)
				WriteConditionalExpression.Go(writer, node.As<ConditionalExpressionSyntax>());
			else if (node is BaseExpressionSyntax)
				WriteBaseExpression.Go(writer, node.As<BaseExpressionSyntax>());
			else if (node is ThisExpressionSyntax)
				WriteThisExpression.Go(writer, node.As<ThisExpressionSyntax>());
			else if (node is CastExpressionSyntax)
				WriteCastExpression.Go(writer, node.As<CastExpressionSyntax>());
			else if (node is ThrowStatementSyntax)
				WriteThrowStatement.Go(writer, node.As<ThrowStatementSyntax>());
			else if (node is PrefixUnaryExpressionSyntax)
				WriteUnaryExpression.Go(writer, node.As<PrefixUnaryExpressionSyntax>());
			else if (node is PostfixUnaryExpressionSyntax)
				WriteUnaryExpression.Go(writer, node.As<PostfixUnaryExpressionSyntax>());
			else if (node is EqualsValueClauseSyntax)
				WriteEqualsValueClause.Go(writer, node.As<EqualsValueClauseSyntax>());
			else if (node is ForStatementSyntax)
				WriteForStatement.Go(writer, node.As<ForStatementSyntax>());
			else if (node is WhileStatementSyntax)
				WriteWhileStatement.Go(writer, node.As<WhileStatementSyntax>());
			else if (node is BreakStatementSyntax)
				WriteBreakStatement.Go(writer, node.As<BreakStatementSyntax>());
			else if (node is DoStatementSyntax)
				WriteDoStatement.Go(writer, node.As<DoStatementSyntax>());
			else if (node is SwitchStatementSyntax)
				WriteSwitchStatement.Go(writer, node.As<SwitchStatementSyntax>());
			else if (node is TryStatementSyntax)
				WriteTryStatement.Go(writer, node.As<TryStatementSyntax>());
			else if (node is UsingStatementSyntax)
				WriteUsingStatement.Go(writer, node.As<UsingStatementSyntax>());
			else if (node is ParenthesizedExpressionSyntax)
				WriteParenthesizedExpression.Go(writer, node.As<ParenthesizedExpressionSyntax>());
			else if (node is LockStatementSyntax)
				Core.Write(writer, node.As<LockStatementSyntax>().Statement); //eat lock statements -- haxe is single-threaded so they do nothing.
			else if (node is ContinueStatementSyntax)
				WriteContinueStatement.Go(writer, node.As<ContinueStatementSyntax>());
			else if (node is TypeOfExpressionSyntax)
				WriteTypeOfExpression.Go(writer, node.As<TypeOfExpressionSyntax>());
			else if (node is AnonymousObjectCreationExpressionSyntax)
				WriteAnonymousObjectCreationExpression.Go(writer, node.As<AnonymousObjectCreationExpressionSyntax>());
			else if (node is EmptyStatementSyntax)
				return; //ignore empty statements
			else if (node is DelegateDeclarationSyntax)
				return; //don't write delegates - we convert them to types directly
			else
				throw new NotImplementedException(node.GetType().Name);
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
