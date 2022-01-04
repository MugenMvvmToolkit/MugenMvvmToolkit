using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ISynchronizedComponent<out T> : IComponent<T> where T : class
    {
        object SyncRoot { get; }
    }
}