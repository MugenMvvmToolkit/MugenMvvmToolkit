using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionManager : IComponentOwner<IComponentCollectionManager>
    {
        IComponentCollection? TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata = null);
    }
}