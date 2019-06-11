using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class AttachedValueProvider : IAttachedValueProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IChildAttachedValueProvider>? _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedValueProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildAttachedValueProvider> Providers
        {
            get
            {
                if (_providers == null)
                    _componentCollectionProvider.LazyInitialize(ref _providers, this);

                return _providers;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedValueProviderDictionary GetAttachedDictionary(object item, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetOrAddAttachedDictionary(item, false, metadata);
        }

        public IAttachedValueProviderDictionary GetAttachedDictionaryOptional(object item, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetOrAddAttachedDictionary(item, true, metadata);
        }

        #endregion

        #region Methods

        private IAttachedValueProviderDictionary? GetOrAddAttachedDictionary(object item, bool optional, IReadOnlyMetadataContext metadata)
        {
            var items = _providers.GetItemsOrDefault();
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].TryGetOrAddAttachedDictionary(this, item, optional, metadata, out var dict))
                    return dict;
            }

            if (!optional)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return null;
        }

        #endregion
    }
}