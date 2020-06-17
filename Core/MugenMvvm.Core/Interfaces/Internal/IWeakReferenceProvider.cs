using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceProvider : IComponentOwner<IWeakReferenceProvider>, IComponent<IMugenApplication>
    {
        IWeakReference? TryGetWeakReference(object? item, IReadOnlyMetadataContext? metadata = null);
    }
}