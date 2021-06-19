using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILocker : IHasPriority
    {
        object SyncRoot { get; }
    }
}