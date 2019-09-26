using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ConditionTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(TokenExpressionParserComponent.IContext context, IExpressionNode? expression,
            IReadOnlyMetadataContext? metadata)
        {
            if (expression == null || !context.SkipWhitespaces().IsToken('?'))
                return null;

            var ifTrue = context.MoveNext().TryParseWhileNotNull(null, metadata);
            if (ifTrue == null || !context.SkipWhitespaces().IsToken(':'))
                return null;

            var ifFalse = context.MoveNext().TryParseWhileNotNull(null, metadata);
            if (ifFalse == null)
                return null;

            return new ConditionExpressionNode(expression, ifTrue, ifFalse);
        }

        #endregion
    }
}