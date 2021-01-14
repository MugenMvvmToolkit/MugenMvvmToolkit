using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Compiling
{
    public interface ICompiledExpression
    {
        object? Invoke(ItemOrArray<ParameterValue> values, IReadOnlyMetadataContext? metadata);
    }
}