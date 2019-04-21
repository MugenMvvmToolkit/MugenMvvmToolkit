using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Components
{
    public class ComponentCollectionProvider : IComponentCollectionProvider
    {
        #region Fields

        private IComponentCollection<IComponentCollectionFactory> _componentCollectionFactories;
        private IComponentCollection<IComponentCollectionProviderListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ComponentCollectionProvider()
        {
        }

        #endregion

        #region Properties

        public IComponentCollection<IComponentCollectionProviderListener> Listeners
        {
            get
            {
                if (_listeners == null && MugenExtensions.LazyInitialize(ref _listeners, this, this))
                    _listeners.AddListener(new ComponentCollectionCallbackListener());
                return _listeners;
            }
        }

        public IComponentCollection<IComponentCollectionFactory> ComponentCollectionFactories
        {
            get
            {
                if (_componentCollectionFactories == null)
                    MugenExtensions.LazyInitialize(ref _componentCollectionFactories, this, this);
                return _componentCollectionFactories;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IComponentCollection<T> GetComponentCollection<T>(object owner, IReadOnlyMetadataContext metadata) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(metadata, nameof(metadata));

            var result = GetComponentCollectionInternal<T>(owner, metadata);

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IComponentCollectionFactory).Name);

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnComponentCollectionCreated(this, result, metadata);

            return result;
        }

        #endregion

        #region Methods

        protected virtual IComponentCollection<T> GetComponentCollectionInternal<T>(object owner, IReadOnlyMetadataContext metadata) where T : class
        {
            var collectionFactories = ComponentCollectionFactories.GetItems();
            for (var i = 0; i < collectionFactories.Length; i++)
            {
                var collection = collectionFactories[i].TryGetComponentCollection<T>(owner, metadata);
                if (collection != null)
                    return collection;
            }

            if (typeof(IHasPriority).IsAssignableFromUnified(typeof(T)) || typeof(IListener).IsAssignableFromUnified(typeof(T)))
                return new OrderedArrayComponentCollection<T>(owner);
            return new ArrayComponentCollection<T>(owner);
        }

        protected IComponentCollectionProviderListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}