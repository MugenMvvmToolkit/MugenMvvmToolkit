using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IWeakEventListener : IEventListener, IWeakItem
    {
        bool IsWeak { get; }
    }
}