using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ParenExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingParserPriority.Paren;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var p = context.Position;
            var position = context.SkipWhitespacesPosition();
            if (!context.IsToken('(', position))
                return null;

            context.SetPosition(position + 1);
            var node = context.TryParseWhileNotNull(expression, metadata);
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