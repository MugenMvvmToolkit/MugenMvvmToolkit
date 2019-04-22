namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReference
    {
        object? Target { get; }

        void Release();
    }
}