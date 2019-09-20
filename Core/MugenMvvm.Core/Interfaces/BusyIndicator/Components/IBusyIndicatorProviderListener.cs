using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.BusyIndicator.Components
{
    public interface IBusyIndicatorProviderListener : IComponent<IBusyIndicatorProvider>
    {
        void OnBeginBusy(IBusyIndicatorProvider busyIndicatorProvider, IBusyInfo busyInfo);

        void OnBusyInfoChanged(IBusyIndicatorProvider busyIndicatorProvider);
    }
}