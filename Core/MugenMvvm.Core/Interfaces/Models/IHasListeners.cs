using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasListeners<T> where T : class, IListener
    {
        IComponentCollection<T> Listeners { get; }
    }
}