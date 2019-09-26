using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface ICompiledExpression
    {
        object? Invoke(object?[] values, IReadOnlyMetadataContext? metadata);
    }
}