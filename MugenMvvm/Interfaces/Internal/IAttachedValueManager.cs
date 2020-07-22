using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueManager : IComponentOwner<IAttachedValueManager>
    {
        AttachedValueStorage TryGetAttachedValues(object item, IReadOnlyMetadataContext? metadata = null);
    }
}