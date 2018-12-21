namespace MugenMvvm.Interfaces.Models
{
    public interface IThreadDispatcherHandler
    {
        void Execute(object? state);
    }
}