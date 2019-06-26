using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProvider : IComponentOwner<IComponentCollectionProvider>
    {
        IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class;
    }
}