namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IEventListener
    {
        bool IsAlive { get; }

        bool IsWeak { get; }

        bool TryHandle(object sender, object? message);
    }
}