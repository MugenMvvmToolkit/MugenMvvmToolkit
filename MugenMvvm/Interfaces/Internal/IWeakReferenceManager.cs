using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceManager : IComponentOwner<IWeakReferenceManager>, IComponent<IMugenApplication>
    {
        IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null);
    }
}