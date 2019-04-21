using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionFactory : IHasPriority
    {
        IComponentCollection<T>? TryGetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class;
    }
}