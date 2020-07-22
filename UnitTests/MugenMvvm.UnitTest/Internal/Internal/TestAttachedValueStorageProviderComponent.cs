using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestAttachedValueStorageProviderComponent : IAttachedValueStorageProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;

        #endregion

        #region Constructors

        public TestAttachedValueStorageProviderComponent(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
        }

        #endregion

        #region Properties

        public TryGetAttachedValuesDelegate? TryGetAttachedValues { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        AttachedValueStorage IAttachedValueStorageProviderComponent.TryGetAttachedValues(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            _attachedValueManager?.ShouldEqual(attachedValueManager);
            if (TryGetAttachedValues == null)
                return default;
            return TryGetAttachedValues(item, metadata);
        }

        #endregion

        #region Nested types

        public delegate AttachedValueStorage TryGetAttachedValuesDelegate(object item, IReadOnlyMetadataContext? metadata);

        #endregion
    }
}