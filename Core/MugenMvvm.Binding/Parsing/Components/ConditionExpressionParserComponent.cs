using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public class ConditionExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode TryParse(IBindingParserContext context, IExpressionNode expression, IReadOnlyMetadataContext metadata)
        {
            if (expression == null)
                return null;

            var p = context.Position;
            var position = context.SkipWhitespaces();
            if (!context.IsToken('?', position))
                return null;

            context.MoveNext();
            var ifTrue = context.ParseWhileToken(':', position, null, metadata);
            if (!context.IsToken(':', null))
            {
                context.SetPosition(p);
                return null;
            }

            context.MoveNext();

            var ifFalse = context.TryParseWhileNotNull(null, metadata);
            if (ifFalse == null)
            {
                context.SetPosition(p);
                return null;
            }

            return new ConditionExpressionNode(expression, ifTrue, ifFalse);
        }

        #endregion
    }
}