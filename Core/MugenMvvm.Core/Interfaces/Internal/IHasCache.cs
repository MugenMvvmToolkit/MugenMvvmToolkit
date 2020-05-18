using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IHasCache
    {
        void Invalidate<TState>([AllowNull]in TState state, IReadOnlyMetadataContext? metadata);
    }
}