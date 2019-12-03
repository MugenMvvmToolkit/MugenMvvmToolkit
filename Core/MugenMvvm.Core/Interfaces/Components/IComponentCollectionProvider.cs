using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProvider : IComponentOwner<IComponentCollectionProvider>, IComponent<IMugenApplication>
    {
        IComponentCollection GetComponentCollection(object owner, IReadOnlyMetadataContext? metadata = null);
    }
}