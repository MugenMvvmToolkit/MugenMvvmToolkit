using System;
using System.Collections.Generic;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Wrapping
{
    public class WrapperManager : HasListenersBase<IWrapperManagerListener>, IWrapperManager
    {
        #region Constructors

        public WrapperManager()
        {
            Factories = new List<IWrapperManagerFactory>();
        }

        #endregion

        #region Properties

        protected List<IWrapperManagerFactory> Factories { get; }

        #endregion

        #region Implementation of interfaces

        public void AddWrapperFactory(IWrapperManagerFactory factory)
        {
            Should.NotBeNull(factory, nameof(factory));
            AddWrapperFactoryInternal(factory);
        }

        public void RemoveWrapperFactory(IWrapperManagerFactory factory)
        {
            Should.NotBeNull(factory, nameof(factory));
            RemoveWrapperFactoryInternal(factory);
        }

        public IReadOnlyList<IWrapperManagerFactory> GetWrapperFactories()
        {
            return GetWrapperFactoriesInternal();
        }

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

        protected virtual void AddWrapperFactoryInternal(IWrapperManagerFactory factory)
        {
            lock (Factories)
            {
                Factories.Add(factory);
            }
        }

        protected virtual void RemoveWrapperFactoryInternal(IWrapperManagerFactory factory)
        {
            lock (Factories)
            {
                Factories.Remove(factory);
            }
        }

        protected virtual IReadOnlyList<IWrapperManagerFactory> GetWrapperFactoriesInternal()
        {
            lock (Factories)
            {
                return Factories.ToArray();
            }
        }

        protected virtual bool CanWrapInternal(Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            if (wrapperType.IsAssignableFromUnified(type))
                return true;

            lock (Factories)
            {
                for (int i = 0; i < Factories.Count; i++)
                {
                    if (Factories[i].CanWrap(this, type, wrapperType, metadata))
                        return true;
                }
            }

            return false;
        }

        protected virtual object WrapInternal(object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            object wrapper = null;
            lock (Factories)
            {
                for (int i = 0; i < Factories.Count; i++)
                {
                    wrapper = Factories[i].TryWrap(this, item.GetType(), wrapperType, metadata);
                    if (wrapper != null)
                        break;
                }
            }
            if (wrapper == null)
                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);

            var listeners = GetListenersInternal();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i]?.OnWrapped(this, item, wrapperType, wrapper, metadata);

            return wrapper;
        }

        #endregion
    }
}