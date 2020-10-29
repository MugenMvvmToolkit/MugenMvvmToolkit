using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Bindings.Interfaces.Observation
{
    public interface IWeakEventListener : IEventListener, IWeakItem
    {
        bool IsWeak { get; }
    }
}