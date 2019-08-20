using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class IndexerExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = 1000;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var node = TryParseInternal(context, expression, metadata);
            if (node == null)
                context.SetPosition(p);
            return node;
        }

        #endregion

        #region Methods

        private static IExpressionNode? TryParseInternal(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
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