using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IHasCache
    {
        void Invalidate(object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}