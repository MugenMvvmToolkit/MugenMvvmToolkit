using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingBuilder : IHasTargetExpressionBindingBuilder
    {
        public Func<object, object?, IReadOnlyMetadataContext?, IBinding>? Build { get; set; }

        public IExpressionNode TargetExpression { get; set; } = null!;

        IBinding IBindingBuilder.Build(object target, object? source, IReadOnlyMetadataContext? metadata) => Build?.Invoke(target, source, metadata)!;
    }
}