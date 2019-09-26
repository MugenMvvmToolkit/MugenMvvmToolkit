using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class IndexerTokenParserComponent : TokenExpressionParserComponent.ITokenExpressionParser, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Indexer;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(TokenExpressionParserComponent.ITokenExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(TokenExpressionParserComponent.ITokenExpressionParserContext context, IExpressionNode? expression,
            IReadOnlyMetadataContext? metadata)
        {
            if (!context.SkipWhitespaces().IsToken('['))
                return null;

            var args = context
                .MoveNext()
                .SkipWhitespaces()
                .ParseArguments("]", metadata);
            if (args == null)
                return null;
            return new IndexExpressionNode(expression, args);
        }

        #endregion
    }
}