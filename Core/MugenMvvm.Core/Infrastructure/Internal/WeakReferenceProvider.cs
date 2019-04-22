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
        private IComponentCollection<IWeakReferenceFactory>? _weakReferenceFactories;

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

        public IComponentCollection<IWeakReferenceFactory> WeakReferenceFactories
        {
            get
            {
                if (_weakReferenceFactories == null)
                    _componentCollectionProvider.LazyInitialize(ref _weakReferenceFactories, this);
                return _weakReferenceFactories;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IWeakReference GetWeakReference(object item, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(metadata, nameof(metadata));

            var factories = WeakReferenceFactories.GetItems();
            for (var i = 0; i < factories.Length; i++)
            {
                var weakReference = factories[i].TryGetWeakReference(this, item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            ExceptionManager.ThrowObjectNotInitialized(this, typeof(IWeakReferenceFactory).Name);
            return null;
        }

        #endregion
    }
}