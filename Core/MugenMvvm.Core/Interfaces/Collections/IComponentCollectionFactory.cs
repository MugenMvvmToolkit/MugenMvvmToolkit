using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IComponentCollectionFactory
    {
        IComponentCollection<T> GetComponentCollection<T>(object target, IReadOnlyMetadataContext metadata) where T : class;
    }
}