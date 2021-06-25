﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ExpressionParserResult
    {
        public readonly IReadOnlyMetadataContext? Metadata;
        public readonly IExpressionNode? Source;
        public readonly IExpressionNode? Target;
        private readonly object? _parametersRaw;

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

        public ExpressionParserResult(IExpressionNode target, IExpressionNode source, ItemOrIReadOnlyList<IExpressionNode> parameters,
            IMetadataOwner<IReadOnlyMetadataContext> context)
            : this(target, source, parameters, context.GetMetadataOrDefault())
        {
        }

        [MemberNotNullWhen(false, nameof(Target))]
        public bool IsEmpty => Target == null;

        public ItemOrIReadOnlyList<IExpressionNode> Parameters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrIReadOnlyList.FromRawValue<IExpressionNode>(_parametersRaw);
        }
    }
}