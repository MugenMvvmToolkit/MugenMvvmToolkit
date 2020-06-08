using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IEventListener : IWeakItem
    {
        bool IsWeak { get; }

        bool TryHandle<T>(object? sender, in T message);
    }
}