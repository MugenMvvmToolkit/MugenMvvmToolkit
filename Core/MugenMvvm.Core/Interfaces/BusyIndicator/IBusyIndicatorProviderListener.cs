namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyIndicatorProviderListener
    {
        void OnBeginBusy(IBusyIndicatorProvider busyIndicatorProvider, IBusyInfo busyInfo);

        void OnBusyInfoChanged(IBusyIndicatorProvider busyIndicatorProvider);
    }
}