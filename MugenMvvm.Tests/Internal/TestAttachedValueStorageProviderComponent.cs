using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestAttachedValueStorageProviderComponent : IAttachedValueStorageProviderComponent, IHasPriority
    {
        public TryGetAttachedValuesDelegate? TryGetAttachedValues { get; set; }

        public int Priority { get; set; }

        AttachedValueStorage IAttachedValueStorageProviderComponent.TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item,
            IReadOnlyMetadataContext? metadata) =>
            TryGetAttachedValues == null ? default : TryGetAttachedValues(attachedValueManager, item, metadata);

        public delegate AttachedValueStorage TryGetAttachedValuesDelegate(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata);
    }
}