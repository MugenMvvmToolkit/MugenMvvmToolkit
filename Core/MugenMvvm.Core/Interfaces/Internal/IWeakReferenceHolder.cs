namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceHolder
    {
        IWeakReference? WeakReference { get; set; }
    }
}