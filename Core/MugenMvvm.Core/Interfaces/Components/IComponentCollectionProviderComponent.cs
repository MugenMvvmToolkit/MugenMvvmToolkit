using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderComponent : IComponent<IComponentCollectionProvider>
    {
        IComponentCollection<T>? TryGetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class;
    }
}