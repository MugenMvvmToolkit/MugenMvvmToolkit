using System;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Wrapping
{
    public class WrapperManager : IWrapperManager
    {
        #region Fields

        private IComponentCollection<IWrapperManagerListener>? _listeners;
        private IComponentCollection<IWrapperManagerFactory>? _wrapperFactories;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public WrapperManager(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public IComponentCollection<IWrapperManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IComponentCollection<IWrapperManagerFactory> WrapperFactories
        {
            get
            {
                if (_wrapperFactories == null)
                    ComponentCollectionProvider.LazyInitialize(ref _wrapperFactories, this);
                return _wrapperFactories;
            }
        }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(metadata, nameof(metadata));
            return CanWrapInternal(type, wrapperType, metadata);
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(metadata, nameof(metadata));
            return WrapInternal(item, wrapperType, metadata);
        }

        #endregion

        #region Methods

        protected virtual bool CanWrapInternal(Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            if (wrapperType.IsAssignableFromUnified(type))
                return true;

            var factories = WrapperFactories.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                if (factories[i].CanWrap(this, type, wrapperType, metadata))
                    return true;
            }

            return false;
        }

        protected virtual object WrapInternal(object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            object? wrapper = null;
            var factories = WrapperFactories.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                wrapper = factories[i].TryWrap(this, item.GetType(), wrapperType, metadata);
                if (wrapper != null)
                    break;
            }

            if (wrapper == null)
                ExceptionManager.ThrowWrapperTypeNotSupported(wrapperType);

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnWrapped(this, item, wrapperType, wrapper!, metadata);

            return wrapper!;
        }

        protected IWrapperManagerListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}