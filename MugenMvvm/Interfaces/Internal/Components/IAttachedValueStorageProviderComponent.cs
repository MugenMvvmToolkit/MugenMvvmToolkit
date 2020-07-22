using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueStorageProviderComponent : IComponent<IAttachedValueManager>
    {
        AttachedValueStorage TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata);
    }
}