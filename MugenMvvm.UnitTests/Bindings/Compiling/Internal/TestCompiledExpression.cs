using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
{
    public class TestCompiledExpression : ICompiledExpression
    {
        #region Properties

        public Func<ItemOrArray<ParameterValue>, IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ICompiledExpression.Invoke(ItemOrArray<ParameterValue> values, IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(values, metadata);

        #endregion
    }
}