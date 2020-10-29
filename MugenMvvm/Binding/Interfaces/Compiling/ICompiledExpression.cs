using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Interfaces.Compiling
{
    public interface ICompiledExpression : IDisposable
    {
        object? Invoke(ItemOrList<ParameterValue, ParameterValue[]> values, IReadOnlyMetadataContext? metadata);
    }
}