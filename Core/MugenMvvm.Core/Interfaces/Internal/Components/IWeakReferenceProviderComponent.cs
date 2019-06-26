using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceProviderComponent : IComponent<IWeakReferenceProvider>
    {
        IWeakReference? TryGetWeakReference(object item, IReadOnlyMetadataContext metadata);
    }
}