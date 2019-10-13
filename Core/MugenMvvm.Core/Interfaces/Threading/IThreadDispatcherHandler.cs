namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcherHandler//todo cache
    {
        void Execute(object? state);
    }
}