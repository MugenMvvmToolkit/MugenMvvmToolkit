using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core
{
    public interface IValueExpression
    {
        object? Invoke(IReadOnlyMetadataContext? metadata = null);
    }
}