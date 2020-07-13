using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}