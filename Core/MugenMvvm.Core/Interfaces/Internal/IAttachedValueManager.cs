using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueManager : IComponentOwner<IAttachedValueManager>, IComponent<IMugenApplication>//todo rename
    {
        IAttachedValueProvider? GetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null);

        IAttachedValueProvider GetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null);
    }
}