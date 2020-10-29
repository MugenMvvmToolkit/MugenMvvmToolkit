using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IEventListener
    {
        bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata);
    }
}