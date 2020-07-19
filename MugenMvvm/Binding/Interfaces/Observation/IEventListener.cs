using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IEventListener
    {
        bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata);
    }
}