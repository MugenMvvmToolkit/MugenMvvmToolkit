using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParserResult
    {
        #region Fields

        public readonly IReadOnlyMetadataContext Metadata;
        public readonly ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> Parameters;
        public readonly IExpressionNode? SourceExpression;
        public readonly IExpressionNode TargetExpression;

        #endregion

        #region Constructors

        public BindingParserResult(IExpressionNode targetExpression, IExpressionNode? sourceExpression, ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(targetExpression, nameof(targetExpression));
            TargetExpression = targetExpression;
            SourceExpression = sourceExpression;
            Parameters = parameters;
            Metadata = metadata ?? Default.Metadata;
        }

        public BindingParserResult(IExpressionNode targetExpression, IExpressionNode? sourceExpression, ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters,
            IBindingParserContext context)
            : this(targetExpression, sourceExpression, parameters, context.HasMetadata ? context.Metadata : Default.Metadata)
        {
        }

        #endregion
    }
}