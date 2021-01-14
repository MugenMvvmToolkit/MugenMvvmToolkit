using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestAttachedValueStorageProviderComponent : IAttachedValueStorageProviderComponent, IHasPriority
    {
        private readonly IAttachedValueManager? _attachedValueManager;

        public TestAttachedValueStorageProviderComponent(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        public delegate AttachedValueStorage TryGetAttachedValuesDelegate(object item, IReadOnlyMetadataContext? metadata);

        public TryGetAttachedValuesDelegate? TryGetAttachedValues { get; set; }

        public int Priority { get; set; }

        AttachedValueStorage IAttachedValueStorageProviderComponent.TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item,
            IReadOnlyMetadataContext? metadata)
        {
            _attachedValueManager?.ShouldEqual(attachedValueManager);
            if (TryGetAttachedValues == null)
                return default;
            return TryGetAttachedValues(item, metadata);
        }
    }
}