using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Interfaces.Observation
{
    public interface IWeakEventListener : IEventListener, IWeakItem
    {
        bool IsWeak { get; }
    }
}