using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISupportChangeNotification
    {
        void RaiseChanged(bool force = false, IReadOnlyMetadataContext? metadata = null);
    }
}