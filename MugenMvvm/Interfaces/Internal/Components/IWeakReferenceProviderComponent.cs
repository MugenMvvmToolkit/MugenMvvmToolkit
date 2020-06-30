using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IWeakReferenceProviderComponent : IComponent<IWeakReferenceManager>
    {
        IWeakReference? TryGetWeakReference(object item, IReadOnlyMetadataContext? metadata);
    }
}