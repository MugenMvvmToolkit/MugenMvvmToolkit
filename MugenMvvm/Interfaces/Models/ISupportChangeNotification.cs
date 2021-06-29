using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface ISupportChangeNotification
    {
        event EventHandler Changed;

        void RaiseChanged(bool force = false, IReadOnlyMetadataContext? metadata = null);
    }
}