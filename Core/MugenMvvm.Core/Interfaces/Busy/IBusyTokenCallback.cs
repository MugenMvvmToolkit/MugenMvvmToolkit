namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyTokenCallback
    {
        void OnCompleted(IBusyToken token);

        void OnSuspendChanged(bool suspended);
    }
}