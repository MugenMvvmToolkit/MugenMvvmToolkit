namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyIndicatorProviderListener
    {
        void OnBeginBusy(IBusyInfo busyInfo);

        void OnBusyInfoChanged();
    }
}