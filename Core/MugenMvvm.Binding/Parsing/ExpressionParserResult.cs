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
        public readonly ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> Parameters;
        public readonly IExpressionNode Source;
        public readonly IExpressionNode Target;

        #endregion

        #region Constructors

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            Target = target;
            Source = source;
            Parameters = parameters;
            Metadata = metadata ?? Default.Metadata;
        }

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters,
            IMetadataOwner<IReadOnlyMetadataContext> context)
            : this(target, source, parameters, context.HasMetadata ? context.Metadata : Default.Metadata)
        {
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        #endregion
    }
}