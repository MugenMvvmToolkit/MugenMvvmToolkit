using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ExpressionParserResult
    {
        #region Fields

        public readonly IReadOnlyMetadataContext Metadata;
        public readonly IExpressionNode Source;
        public readonly IExpressionNode Target;
        private readonly object? _parametersRaw;

        #endregion

        #region Constructors

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            Target = target;
            Source = source;
            _parametersRaw = parameters.GetRawValue();
            Metadata = metadata ?? Default.Metadata;
        }

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters,
            IMetadataOwner<IReadOnlyMetadataContext> context)
            : this(target, source, parameters, context.HasMetadata ? context.Metadata : Default.Metadata)
        {
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        public ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> Parameters => ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>>.FromRawValue(_parametersRaw);

        #endregion
    }
}