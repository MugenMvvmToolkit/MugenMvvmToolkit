using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MemberExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryParse(IBindingParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata)
        {
            var position = context.SkipWhitespaces();
            if (expression != null)
            {
                if (!context.IsToken('.', position))
                    return null;
                ++position;
            }

            if (!context.IsIdentifier(position, out var endPosition))
                return null;

            context.SetPosition(endPosition);
            return new MemberExpressionNode(expression, context.GetValue(position, endPosition));
        }

        #endregion
    }
}