using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IEventListener
    {
        bool TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata);
    }
}