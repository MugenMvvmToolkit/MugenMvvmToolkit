using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class NullConditionalMemberTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Member;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression == null)
                return null;

            context.SkipWhitespaces();
            if (context.IsToken('?') && !context.IsToken("??"))
            {
                context.MoveNext();
                return context.TryParse(new NullConditionalMemberExpressionNode(expression));
            }

            return null;
        }

        #endregion
    }
}