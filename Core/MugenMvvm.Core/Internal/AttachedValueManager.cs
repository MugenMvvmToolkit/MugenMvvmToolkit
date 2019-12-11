using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedValueManager : ComponentOwnerBase<IAttachedValueManager>, IAttachedValueManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedValueManager(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedValueProvider GetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var provider = GetComponents<IAttachedValueManagerComponent>(metadata).TryGetOrAddAttachedValueProvider(item, metadata);
            if (provider == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return provider;
        }

        public IAttachedValueProvider? GetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            GetComponents<IAttachedValueManagerComponent>(metadata).TryGetAttachedValueProvider(item, metadata, out var provider);
            return provider;
        }

        #endregion
    }
}