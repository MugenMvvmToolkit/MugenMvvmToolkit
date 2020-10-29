using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingBuilder : IHasTargetExpressionBindingBuilder
    {
        #region Properties

        public Func<object, object?, IReadOnlyMetadataContext?, IBinding>? Build { get; set; }

        public IExpressionNode TargetExpression { get; set; } = null!;

        #endregion

        #region Implementation of interfaces

        IBinding IBindingBuilder.Build(object target, object? source, IReadOnlyMetadataContext? metadata) => Build?.Invoke(target, source, metadata)!;

        #endregion
    }
}