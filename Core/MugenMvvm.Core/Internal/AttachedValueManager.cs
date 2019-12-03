using MugenMvvm.Attributes;
using MugenMvvm.Components;
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
            var components = GetComponents<IAttachedValueManagerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetOrAddAttachedValueProvider(item, metadata, out var provider))
                    return provider!;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IAttachedValueManagerComponent).Name);
            return null!;
        }

        public IAttachedValueProvider? GetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var components = GetComponents<IAttachedValueManagerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out var provider))
                    return provider;
            }

            return null;
        }

        #endregion
    }
}