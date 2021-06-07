using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
{
    public class TestCompiledExpression : ICompiledExpression
    {
        public Func<ItemOrArray<ParameterValue>, IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        object? ICompiledExpression.Invoke(ItemOrArray<ParameterValue> values, IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(values, metadata);
    }
}