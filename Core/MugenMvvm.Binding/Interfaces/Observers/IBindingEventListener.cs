namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingEventListener
    {
        bool IsAlive { get; }

        bool IsWeak { get; }

        bool TryHandle(object sender, object? message);
    }
}