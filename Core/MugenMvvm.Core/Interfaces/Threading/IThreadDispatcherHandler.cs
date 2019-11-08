namespace MugenMvvm.Interfaces.Threading
{
    public interface IThreadDispatcherHandler<TState>
    {
        void Execute(TState state);
    }
}