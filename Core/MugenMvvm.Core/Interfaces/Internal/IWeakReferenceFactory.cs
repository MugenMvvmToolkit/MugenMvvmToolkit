using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceFactory : IHasPriority//todo factory to child?
    {
        IWeakReference? TryGetWeakReference(IWeakReferenceProvider provider, object item, IReadOnlyMetadataContext metadata);
    }
}