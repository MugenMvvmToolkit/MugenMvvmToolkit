using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildWeakReferenceProvider : IHasPriority
    {
        IWeakReference? TryGetWeakReference(IWeakReferenceProvider provider, object item, IReadOnlyMetadataContext metadata);
    }
}