using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class AttachedValueManager : ComponentOwnerBase<IAttachedValueManager>, IAttachedValueManager,
        IComponentOwnerAddedCallback<IComponent<IAttachedValueManager>>, IComponentOwnerRemovedCallback<IComponent<IAttachedValueManager>>
    {
        #region Fields

        private IAttachedValueManagerComponent[] _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AttachedValueManager(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            _components = Default.EmptyArray<IAttachedValueManagerComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public IAttachedValueProvider GetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var components = _components;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetOrAddAttachedValueProvider(item, metadata, out var dict))
                    return dict!;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IAttachedValueManagerComponent).Name);
            return null!;
        }

        public IAttachedValueProvider? GetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(item, nameof(item));
            var components = _components;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out var dict))
                    return dict;
            }

            return null;
        }

        void IComponentOwnerAddedCallback<IComponent<IAttachedValueManager>>.OnComponentAdded(IComponentCollection<IComponent<IAttachedValueManager>> collection,
            IComponent<IAttachedValueManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _components, collection, component);
        }

        void IComponentOwnerRemovedCallback<IComponent<IAttachedValueManager>>.OnComponentRemoved(IComponentCollection<IComponent<IAttachedValueManager>> collection,
            IComponent<IAttachedValueManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _components, component);
        }

        #endregion
    }
}