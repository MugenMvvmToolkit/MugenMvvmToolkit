using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Metadata
{
    public sealed class MetadataContextProvider : IMetadataContextProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IMetadataContextProviderListener>? _listeners;
        private IComponentCollection<IMetadataContextFactory>? _metadataContextFactories;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MetadataContextProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IMetadataContextProviderListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _componentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        public IComponentCollection<IMetadataContextFactory> MetadataContextFactories
        {
            get
            {
                if (_metadataContextFactories == null)
                    _componentCollectionProvider.LazyInitialize(ref _metadataContextFactories, this);
                return _metadataContextFactories;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var factories = MetadataContextFactories.GetItems();
            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < factories.Length; i++)
            {
                result = factories[i].TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextFactory).Name);

            var listeners = _listeners.GetItemsOrDefault();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReadOnlyContextCreated(this, result, target);
            return result;
        }

        public IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var factories = MetadataContextFactories.GetItems();
            IMetadataContext? result = null;
            for (var i = 0; i < factories.Length; i++)
            {
                result = factories[i].TryGetMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextFactory).Name);

            var listeners = _listeners.GetItemsOrDefault();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnContextCreated(this, result, target);
            return result;
        }

        public IObservableMetadataContext GetObservableMetadataContext(object? target, IEnumerable<MetadataContextValue>? values)
        {
            var factories = MetadataContextFactories.GetItems();
            IObservableMetadataContext? result = null;
            for (var i = 0; i < factories.Length; i++)
            {
                result = factories[i].TryGetObservableMetadataContext(target, values);
                if (result != null)
                    break;
            }

            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(IMetadataContextFactory).Name);

            var listeners = _listeners.GetItemsOrDefault();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnObservableContextCreated(this, result, target);
            return result;
        }

        #endregion
    }
}