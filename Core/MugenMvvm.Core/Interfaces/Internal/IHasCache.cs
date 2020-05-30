using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IHasCache
    {
        void Invalidate<TState>(in TState state, IReadOnlyMetadataContext? metadata);
    }
}