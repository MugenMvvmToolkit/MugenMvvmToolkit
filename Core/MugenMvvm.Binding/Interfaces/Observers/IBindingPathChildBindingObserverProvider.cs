using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingPathChildBindingObserverProvider : IChildBindingObserverProvider
    {
        IBindingPath? TryGetBindingPath(object path, IReadOnlyMetadataContext metadata);

        IBindingPathObserver? TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext metadata);
    }
}