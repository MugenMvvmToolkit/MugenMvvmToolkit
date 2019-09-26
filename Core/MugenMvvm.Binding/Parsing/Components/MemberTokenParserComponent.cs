using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MemberTokenParserComponent : TokenExpressionParserComponent.ITokenExpressionParser, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Member;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.ITokenExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var position = context.SkipWhitespacesPosition();
            if (expression != null)
            {
                if (!context.IsToken('.', position))
                    return null;
                ++position;
            }

            if (!context.IsIdentifier(out var endPosition, position))
                return null;

            context.SetPosition(endPosition);
            return new MemberExpressionNode(expression, context.GetValue(position, endPosition));
        }

        #endregion
    }
}