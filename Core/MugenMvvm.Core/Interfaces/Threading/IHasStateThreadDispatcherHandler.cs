namespace MugenMvvm.Interfaces.Threading
{
    public interface IHasStateThreadDispatcherHandler : IThreadDispatcherHandler
    {
        object State { get; set; }
    }
}