using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceProvider
    {
        IComponentCollection<IWeakReferenceFactory> WeakReferenceFactories { get; }

        IWeakReference GetWeakReference(object item, IReadOnlyMetadataContext metadata);
    }
}