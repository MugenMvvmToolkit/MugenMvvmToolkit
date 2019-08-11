using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedDictionaryProvider : ComponentOwnerBase<IAttachedDictionaryProvider>, IAttachedDictionaryProvider,
        IComponentOwnerAddedCallback<IComponent<IAttachedDictionaryProvider>>, IComponentOwnerRemovedCallback<IComponent<IAttachedDictionaryProvider>>
    {
        #region Fields

        private IAttachedDictionaryProviderComponent[] _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedDictionaryProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _components = Default.EmptyArray<IAttachedDictionaryProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedDictionary GetOrAddAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < _components.Length; i++)
            {
                if (_components[i].TryGetOrAddAttachedDictionary(item, metadata, out var dict))
                    return dict;
            }

            ExceptionManager.ThrowObjectNotInitialized(this);
            return null!;
        }

        public IAttachedDictionary? GetAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < _components.Length; i++)
            {
                if (_components[i].TryGetAttachedDictionary(item, metadata, out var dict))
                    return dict;
            }

            return null;
        }

        void IComponentOwnerAddedCallback<IComponent<IAttachedDictionaryProvider>>.OnComponentAdded(IComponentCollection<IComponent<IAttachedDictionaryProvider>> collection,
            IComponent<IAttachedDictionaryProvider> component, IReadOnlyMetadataContext metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _components, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IAttachedDictionaryProvider>>.OnComponentRemoved(IComponentCollection<IComponent<IAttachedDictionaryProvider>> collection,
            IComponent<IAttachedDictionaryProvider> component, IReadOnlyMetadataContext metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _components, collection, component, metadata);
        }

        #endregion
    }
}