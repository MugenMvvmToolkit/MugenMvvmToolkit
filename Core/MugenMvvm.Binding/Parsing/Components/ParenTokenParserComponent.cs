using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ParenTokenParserComponent : TokenExpressionParserComponent.IParser, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Paren;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.IContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var p = context.Position;
            var position = context.SkipWhitespacesPosition();
            if (!context.IsToken('(', position))
                return null;

            context.SetPosition(position + 1);
            var node = context.TryParseWhileNotNull();
            if (context.SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return node;
            }

            context.SetPosition(p);
            return null;
        }

        #endregion
    }
}