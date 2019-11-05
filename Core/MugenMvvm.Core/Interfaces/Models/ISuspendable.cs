using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        ActionToken Suspend();
    }
}