using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParserResult
    {
        #region Fields

        public readonly IReadOnlyMetadataContext Metadata;
        public readonly IReadOnlyList<IExpressionNode> Parameters;
        public readonly IExpressionNode? SourceExpression;
        public readonly IExpressionNode TargetExpression;

        #endregion

        #region Constructors

        public BindingParserResult(IExpressionNode targetExpression, IExpressionNode? sourceExpression, IReadOnlyList<IExpressionNode>? parameters, IReadOnlyMetadataContext? metadata)
        {
            TargetExpression = targetExpression;
            SourceExpression = sourceExpression;
            Parameters = parameters ?? Default.EmptyArray<IExpressionNode>();
            Metadata = metadata ?? Default.Metadata;
        }

        public BindingParserResult(IExpressionNode targetExpression, IExpressionNode? sourceExpression, IReadOnlyList<IExpressionNode>? parameters, IBindingParserContext context)
        {
            TargetExpression = targetExpression;
            SourceExpression = sourceExpression;
            Parameters = parameters ?? Default.EmptyArray<IExpressionNode>();
            Metadata = context.HasMetadata ? context.Metadata : Default.Metadata;
        }

        #endregion
    }
}