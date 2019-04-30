using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationContextFactory : AttachableComponentBase<INavigationDispatcher>, INavigationContextFactory
    {
        #region Fields

        private IComponentCollection<INavigationContextFactoryListener>? _listeners;

        #endregion

        #region Constructors

        public NavigationContextFactory(IComponentCollectionProvider componentCollectionProvider, IMetadataContextProvider metadataContextProvider)
        {
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            ComponentCollectionProvider = componentCollectionProvider;
            MetadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider { get; }

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public bool IsListenersInitialized => _listeners != null;

        public IComponentCollection<INavigationContextFactoryListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public INavigationContext GetNavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
            IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationTypeFrom, nameof(navigationTypeFrom));
            Should.NotBeNull(navigationTypeTo, nameof(navigationTypeTo));
            Should.NotBeNull(metadata, nameof(metadata));
            var ctx = GetNavigationContextInternal(navigationProvider, navigationMode, navigationTypeFrom, viewModelFrom, navigationTypeTo, viewModelTo, metadata);
            OnNavigationContextCreated(ctx, metadata);
            return ctx;
        }

        public INavigationContext GetNavigationContextFrom(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            var ctx = GetNavigationContextFromInternal(navigationProvider, navigationMode, navigationType, viewModel, metadata);
            OnNavigationContextCreated(ctx, metadata);
            return ctx;
        }

        public INavigationContext GetNavigationContextTo(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            var ctx = GetNavigationContextToInternal(navigationProvider, navigationMode, navigationType, viewModel, metadata);
            OnNavigationContextCreated(ctx, metadata);
            return ctx;
        }

        #endregion

        #region Methods

        protected virtual INavigationContext GetNavigationContextInternal(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
            IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata)
        {
            return new NavigationContext(navigationProvider, navigationMode, navigationTypeFrom, viewModelFrom, navigationTypeTo, viewModelTo, metadata, MetadataContextProvider);
        }

        protected virtual INavigationContext GetNavigationContextFromInternal(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel, IReadOnlyMetadataContext metadata)
        {
            var entry = GetLastNavigationEntry(navigationType, metadata);
            return GetNavigationContextInternal(navigationProvider, navigationMode, navigationType, viewModel, entry?.NavigationType ?? NavigationType.System, entry?.ViewModel,
                metadata);
        }

        protected virtual INavigationContext GetNavigationContextToInternal(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel, IReadOnlyMetadataContext metadata)
        {
            var entry = GetLastNavigationEntry(navigationType, metadata);
            return GetNavigationContextInternal(navigationProvider, navigationMode, entry?.NavigationType ?? NavigationType.System, entry?.ViewModel, navigationType, viewModel,
                metadata);
        }

        protected virtual INavigationEntry? GetLastNavigationEntry(NavigationType navigationType, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                if (listeners[i].TryGetLastNavigationEntry(navigationType, metadata, out var entry))
                    return entry;
            }

            var list = Owner.NavigationJournal.GetNavigationEntries(navigationType, metadata);
            if (list.Count != 0)
                return list.OrderByDescending(entry => entry.NavigationDate).First();

            list = Owner.NavigationJournal.GetNavigationEntries(navigationType, metadata);
            return list.Where(entry => entry.NavigationType != NavigationType.Tab).OrderByDescending(entry => entry.NavigationDate).FirstOrDefault();
        }

        protected virtual void OnNavigationContextCreated(INavigationContext context, IReadOnlyMetadataContext metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnNavigationContextCreated(this, context, metadata);
        }

        #endregion

        #region Nested types

        protected class NavigationContext : INavigationContext
        {
            #region Fields

            private readonly IMetadataContextProvider _metadataContextProvider;
            private IMetadataContext? _metadata;

            #endregion

            #region Constructors

            public NavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
                IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext? metadata,
                IMetadataContextProvider metadataContextProvider)
            {
                _metadataContextProvider = metadataContextProvider;
                NavigationProvider = navigationProvider;
                NavigationMode = navigationMode;
                NavigationTypeFrom = navigationTypeFrom;
                NavigationTypeTo = navigationTypeTo;
                ViewModelFrom = viewModelFrom;
                ViewModelTo = viewModelTo;
                if (metadata != null)
                {
                    if (metadata is IMetadataContext m)
                        _metadata = m;
                    else
                        _metadata = metadataContextProvider.GetMetadataContext(this, metadata);
                }
            }

            #endregion

            #region Properties

            public bool IsMetadataInitialized => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        _metadataContextProvider.LazyInitialize(ref _metadata, this);
                    return _metadata;
                }
            }

            public NavigationMode NavigationMode { get; }

            public NavigationType NavigationTypeFrom { get; }

            public NavigationType NavigationTypeTo { get; }

            public INavigationProvider NavigationProvider { get; }

            public IViewModelBase? ViewModelFrom { get; }

            public IViewModelBase? ViewModelTo { get; }

            #endregion
        }

        #endregion
    }
}