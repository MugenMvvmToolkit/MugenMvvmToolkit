using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class ParenTokenParser : ITokenParserComponent, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Paren;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            if (expression != null)
                return null;

            var p = context.Position;
            var position = context.SkipWhitespacesPosition();
            if (!context.IsToken('(', position))
                return null;

            context.Position = position + 1;
            var node = context.TryParseWhileNotNull();
            if (context.SkipWhitespaces().IsToken(')'))
            {
                context.MoveNext();
                return node;
            }

            context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseParenExpressionExpectedToken);
            context.Position = p;
            return null;
        }
    }
}