using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IChildComponentCollectionProvider : IHasPriority
    {
        IComponentCollection<T>? TryGetComponentCollection<T>(IComponentCollectionProvider provider, object owner, IReadOnlyMetadataContext metadata) where T : class;
    }
}