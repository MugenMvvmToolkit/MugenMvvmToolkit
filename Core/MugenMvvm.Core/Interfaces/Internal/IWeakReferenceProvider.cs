using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceProvider : IComponentOwner<IWeakReferenceProvider>
    {
        IWeakReference GetWeakReference(object? item, IReadOnlyMetadataContext metadata);
    }
}