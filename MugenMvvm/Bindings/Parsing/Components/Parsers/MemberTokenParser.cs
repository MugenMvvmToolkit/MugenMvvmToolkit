using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class MemberTokenParser : ITokenParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Member;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
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

            context.Position = endPosition;
            return MemberExpressionNode.Get(expression, context.GetValue(position, endPosition));
        }

        #endregion
    }
}