using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public class ObserverProvider : IObserverProvider
    {
        #region Fields

        private IComponentCollection<IChildObserverProvider>? _providers;

        #endregion

        #region Constructors

        public ObserverProvider()
        {
        }

        public ObserverProvider(IComponentCollectionProvider componentCollectionProvider)
        {
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public IComponentCollection<IChildObserverProvider> Providers
        {
            get
            {
                if (_providers == null)
                    ComponentCollectionProvider.LazyInitialize(ref _providers, this);
                return _providers;
            }
        }

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        #endregion

        #region Implementation of interfaces

        public IBindingMemberObserver? TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetMemberObserverInternal(type, member, metadata);
        }

        #endregion

        #region Methods

        protected virtual IBindingMemberObserver? TryGetMemberObserverInternal(Type type, object member, IReadOnlyMetadataContext metadata)
        {
            var items = Providers.GetItems();
            for (var i = 0; i < items.Length; i++)
            {
                var observer = items[i].TryGetMemberObserver(type, member, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}