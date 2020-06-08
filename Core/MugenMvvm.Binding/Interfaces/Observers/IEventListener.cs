using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IEventListener : IWeakItem
    {
        bool IsWeak { get; }

        bool TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata);
    }
}