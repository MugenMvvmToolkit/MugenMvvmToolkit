using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class ConditionTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Condition;

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
            {
                context.TryGetErrors()?.Add(ifTrue == null
                    ? BindingMessageConstant.CannotParseConditionExpressionFormat1.Format(expression)
                    : BindingMessageConstant.CannotParseConditionExpressionExpectedTokenFormat2.Format(expression, ifTrue));
                return null;
            }

            var ifFalse = context.MoveNext().TryParseWhileNotNull();
            if (ifFalse == null)
            {
                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseConditionExpressionFormat2.Format(expression, ifTrue));
                return null;
            }

            return new ConditionExpressionNode(expression, ifTrue, ifFalse, null);
        }

        #endregion
    }
}