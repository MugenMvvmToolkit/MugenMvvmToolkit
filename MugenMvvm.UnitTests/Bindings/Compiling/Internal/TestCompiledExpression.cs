using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
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