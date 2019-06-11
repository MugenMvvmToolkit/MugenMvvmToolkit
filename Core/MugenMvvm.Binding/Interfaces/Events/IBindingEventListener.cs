namespace MugenMvvm.Binding.Interfaces.Events
{
    public interface IBindingEventListener
    {
        bool IsAlive { get; }

        bool IsWeak { get; }

        bool TryHandle(object sender, object message);
    }
}