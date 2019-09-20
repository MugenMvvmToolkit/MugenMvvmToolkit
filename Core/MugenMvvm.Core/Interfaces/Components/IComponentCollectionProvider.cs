using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProvider : IComponentOwner<IComponentCollectionProvider>, IComponent<IMugenApplication>
    {
        IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext? metadata = null) where T : class;
    }
}