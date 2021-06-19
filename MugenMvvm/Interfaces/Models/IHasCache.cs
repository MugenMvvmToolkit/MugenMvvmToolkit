using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasCache
    {
        void Invalidate(object sender, object? state = null, IReadOnlyMetadataContext? metadata = null);
    }
}