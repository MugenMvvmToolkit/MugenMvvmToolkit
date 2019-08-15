using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public class ParenExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = 1000;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode TryParse(IBindingParserContext context, IExpressionNode expression, IReadOnlyMetadataContext metadata)
        {
            var p = context.Position;
            var position = context.SkipWhitespaces();
            if (context.IsToken('(', position))
            {
                context.SetPosition(position + 1);
                var node = context.TryParseWhileNotNull(expression, metadata);
                context.SetPosition(context.SkipWhitespaces());
                if (context.IsToken(')', null))
                {
                    context.MoveNext();
                    return node;
                }

                context.SetPosition(p);
                return null;
            }

            return null;
        }

        #endregion
    }
}