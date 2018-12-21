namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyTokenCallback
    {
        void OnCompleted(IBusyToken token);

        void OnSuspendChanged(bool suspended);
    }
}