using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IExpressionValue
    {
        object? Invoke(IReadOnlyMetadataContext? metadata = null);
    }
}