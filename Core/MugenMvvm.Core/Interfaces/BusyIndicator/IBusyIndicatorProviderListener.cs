using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyIndicatorProviderListener : IListener
    {
        void OnBeginBusy(IBusyIndicatorProvider busyIndicatorProvider, IBusyInfo busyInfo);

        void OnBusyInfoChanged(IBusyIndicatorProvider busyIndicatorProvider);
    }
}