using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedDictionaryProvider : ComponentOwnerBase<IAttachedDictionaryProvider>, IAttachedDictionaryProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedDictionaryProvider(IComponentCollectionProvider componentCollectionProvider) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedDictionary GetOrAddAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IAttachedDictionaryProviderComponent factory && factory.TryGetOrAddAttachedDictionary(item, metadata, out var dict))
                    return dict;
            }

            ExceptionManager.ThrowObjectNotInitialized(this);
            return null!;
        }

        public IAttachedDictionary? GetAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var items = Components.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] is IAttachedDictionaryProviderComponent factory && factory.TryGetAttachedDictionary(item, metadata, out var dict))
                    return dict;
            }

            return null;
        }

        #endregion
    }
}