using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
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

        public WrapperManager(IComponentCollection<IWrapperManagerFactory>? wrapperFactories = null, IComponentCollection<IWrapperManagerListener>? listeners = null)
        {
            _wrapperFactories = wrapperFactories;
            _listeners = listeners;
        }

        #endregion

        #region Properties

        public IComponentCollection<IWrapperManagerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IWrapperManagerListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public IComponentCollection<IWrapperManagerFactory> WrapperFactories
        {
            get
            {
                if (_wrapperFactories == null)
                    _wrapperFactories = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IWrapperManagerFactory>(this, Default.MetadataContext);
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
            for (var i = 0; i < factories.Count; i++)
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
            for (var i = 0; i < factories.Count; i++)
            {
                wrapper = factories[i].TryWrap(this, item.GetType(), wrapperType, metadata);
                if (wrapper != null)
                    break;
            }

            if (wrapper == null)
                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnWrapped(this, item, wrapperType, wrapper, metadata);

            return wrapper;
        }

        protected IReadOnlyList<IWrapperManagerListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IWrapperManagerListener>();
        }

        #endregion
    }
}