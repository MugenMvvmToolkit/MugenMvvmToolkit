using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingExpression : IHasTargetExpressionBindingExpression
    {
        #region Properties

        public Func<object, object?, IReadOnlyMetadataContext?, IBinding>? Build { get; set; }

        public IExpressionNode TargetExpression { get; set; } = null!;

        #endregion

        #region Implementation of interfaces

        IBinding IBindingExpression.Build(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return Build?.Invoke(target, source, metadata)!;
        }

        #endregion
    }
}