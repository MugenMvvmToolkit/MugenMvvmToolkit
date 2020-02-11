namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcherHandler
    {
        void Execute();
    }

    public interface IThreadDispatcherHandler<TState>
    {
        void Execute(TState state);
    }
}