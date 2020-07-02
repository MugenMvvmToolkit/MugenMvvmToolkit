using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceManager : IComponentOwner<IWeakReferenceManager>
    {
        IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null);
    }
}