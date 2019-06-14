using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class WeakReferenceProvider : IWeakReferenceProvider
    {
        #region Fields

        private readonly IComponentCollectionProvider _componentCollectionProvider;
        private IComponentCollection<IChildWeakReferenceProvider>? _providers;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public WeakReferenceProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildWeakReferenceProvider> Providers
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

        public IWeakReference GetWeakReference(object item, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            
            if (item == null)
                return Default.WeakReference;

            if (item is IWeakReference w)
                return w;

            var factories = Providers.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                var weakReference = factories[i].TryGetWeakReference(this, item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IChildWeakReferenceProvider).Name);
            return null;
        }

        #endregion
    }
}