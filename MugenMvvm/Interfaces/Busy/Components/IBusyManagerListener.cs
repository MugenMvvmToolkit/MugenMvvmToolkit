using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerListener : IComponent<IBusyManager>
    {
        void OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata);

        void OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata);
    }
}