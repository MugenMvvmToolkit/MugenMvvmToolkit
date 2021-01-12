using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Parsing
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

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, object? parametersRaw, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            Target = target;
            Source = source;
            _parametersRaw = parametersRaw;
            Metadata = metadata.DefaultIfNull();
        }

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrIReadOnlyList<IExpressionNode> parameters, IReadOnlyMetadataContext? metadata = null)
            : this(target, source, parameters.GetRawValue(), metadata)
        {
        }

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrIReadOnlyList<IExpressionNode> parameters, IMetadataOwner<IReadOnlyMetadataContext> context)
            : this(target, source, parameters, context.GetMetadataOrDefault())
        {
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        public ItemOrIReadOnlyList<IExpressionNode> Parameters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrIReadOnlyList.FromRawValue<IExpressionNode>(_parametersRaw);
        }

        #endregion
    }
}