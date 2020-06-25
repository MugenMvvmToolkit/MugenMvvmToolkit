using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IValueExpression
    {
        object? Invoke(IReadOnlyMetadataContext? metadata = null);
    }
}