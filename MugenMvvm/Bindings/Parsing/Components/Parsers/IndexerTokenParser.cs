using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Parsers
{
    public sealed class IndexerTokenParser : ITokenParserComponent, IHasPriority
    {
        public int Priority { get; init; } = ParsingComponentPriority.Indexer;

        public IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression);
            if (node == null)
                context.Position = p;
            return node;
        }

        private static IExpressionNode? TryParseInternal(ITokenParserContext context, IExpressionNode? expression)
        {
            if (!context.SkipWhitespaces().IsToken('['))
                return null;

            var args = context
                       .MoveNext()
                       .SkipWhitespaces()
                       .ParseArguments("]");
            if (args.IsEmpty)
                return null;
            return new IndexExpressionNode(expression, args);
        }
    }
}