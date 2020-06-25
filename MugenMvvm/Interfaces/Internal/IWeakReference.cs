namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReference : IWeakItem
    {
        object? Target { get; }

        void Release();
    }
}