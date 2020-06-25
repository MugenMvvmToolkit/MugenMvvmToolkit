using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        ActionToken Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata);
    }
}