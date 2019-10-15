using MugenMvvm.Binding.Compiling;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface ICompiledExpression
    {
        object? Invoke(ExpressionValue[] values, IReadOnlyMetadataContext? metadata);//todo values itemorlist
    }
}