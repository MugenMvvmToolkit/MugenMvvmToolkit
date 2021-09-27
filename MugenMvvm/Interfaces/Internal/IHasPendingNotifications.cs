using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IHasPendingNotifications : IComponent
    {
        void Raise(IReadOnlyMetadataContext? metadata = null);
    }
}