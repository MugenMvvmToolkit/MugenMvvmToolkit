namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcherHandler
    {
        void Execute(object? state);
    }
}