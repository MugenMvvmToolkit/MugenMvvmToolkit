using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IBindingExpressionValue
    {
        object? Invoke(IReadOnlyMetadataContext? metadata = null);
    }
}