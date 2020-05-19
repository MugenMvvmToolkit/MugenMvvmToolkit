using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISuspendable
    {
        bool IsSuspended { get; }

        ActionToken Suspend<TState>([AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}