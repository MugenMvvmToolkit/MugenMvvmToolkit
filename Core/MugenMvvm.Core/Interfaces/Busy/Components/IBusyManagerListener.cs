using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Busy.Components
{
    public interface IBusyManagerListener : IComponent<IBusyManager>
    {
        void OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken);

        void OnBusyChanged(IBusyManager busyManager);
    }
}