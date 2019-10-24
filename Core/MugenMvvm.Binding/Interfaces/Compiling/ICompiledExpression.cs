using MugenMvvm.Binding.Compiling;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface ICompiledExpression
    {
        object? Invoke(ItemOrList<ExpressionValue, ExpressionValue[]> values, IReadOnlyMetadataContext? metadata);
    }
}