using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IEventListener
    {
        bool TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata);
    }
}