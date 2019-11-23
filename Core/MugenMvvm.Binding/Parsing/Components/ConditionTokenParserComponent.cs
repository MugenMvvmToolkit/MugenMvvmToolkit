using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ConditionTokenParserComponent : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParserComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression == null || !context.SkipWhitespaces().IsToken('?'))
                return null;

            var ifTrue = context.MoveNext().TryParseWhileNotNull();
            if (ifTrue == null || !context.SkipWhitespaces().IsToken(':'))
                return null;

            var ifFalse = context.MoveNext().TryParseWhileNotNull();
            if (ifFalse == null)
                return null;

            return new ConditionExpressionNode(expression, ifTrue, ifFalse);
        }

        #endregion
    }
}