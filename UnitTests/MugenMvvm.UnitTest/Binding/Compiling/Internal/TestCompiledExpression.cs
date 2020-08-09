using System;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Compiling.Internal
{
    public class TestCompiledExpression : ICompiledExpression
    {
        #region Properties

        public Func<ItemOrList<ParameterValue, ParameterValue[]>, IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        public Action? Dispose { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ICompiledExpression.Invoke(ItemOrList<ParameterValue, ParameterValue[]> values, IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(values, metadata);

        void IDisposable.Dispose() => Dispose?.Invoke();

        #endregion
    }
}