using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IEventListener : IWeakItem
    {
        bool IsWeak { get; }

        bool TryHandle(object? sender, object? message);//todo message generic EventsCollection?
    }
}