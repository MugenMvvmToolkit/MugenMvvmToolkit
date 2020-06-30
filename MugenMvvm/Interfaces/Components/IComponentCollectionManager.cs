using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionManager : IComponentOwner<IComponentCollectionManager>, IComponent<IMugenApplication>
    {
        IComponentCollection? TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata = null);
    }
}