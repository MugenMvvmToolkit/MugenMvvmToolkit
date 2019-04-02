using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationContextFactory : INavigationContextFactory
    {
        #region Properties

        protected INavigationDispatcher NavigationDispatcher { get; private set; }

        #endregion

        #region Implementation of interfaces

        public void OnAttached(INavigationDispatcher owner)
        {
            Should.NotBeNull(owner, nameof(owner));
            NavigationDispatcher = owner;
            OnAttachedInternal(owner);
        }

        public INavigationContext GetNavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
            IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationTypeFrom, nameof(navigationTypeFrom));
            Should.NotBeNull(navigationTypeTo, nameof(navigationTypeTo));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationContextInternal(navigationProvider, navigationMode, navigationTypeFrom, viewModelFrom, navigationTypeTo, viewModelTo, metadata);
        }

        public INavigationContext GetNavigationContextFrom(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationContextFromInternal(navigationProvider, navigationMode, navigationType, viewModel, metadata);
        }

        public INavigationContext GetNavigationContextTo(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationType,
            IViewModelBase? viewModel,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationMode, nameof(navigationMode));
            Should.NotBeNull(navigationType, nameof(navigationType));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationContextToInternal(navigationProvider, navigationMode, navigationType, viewModel, metadata);
        }

        #endregion

        #region Methods

        protected virtual void OnAttachedInternal(INavigationDispatcher navigationDispatcher)
        {
        }

        protected virtual INavigationContext GetNavigationContextInternal(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
            IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext metadata)
        {
            return new NavigationContext(navigationProvider, navigationMode, navigationTypeFrom, viewModelFrom, navigationTypeTo, viewModelTo, metadata);
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

        protected INavigationEntry? GetLastNavigationEntry(NavigationType navigationType, IReadOnlyMetadataContext metadata)
        {
            var list = NavigationDispatcher.NavigationJournal.GetNavigationEntries(navigationType, metadata);
            if (list.Count != 0)
                return list.OrderByDescending(entry => entry.NavigationDate).First();

            list = NavigationDispatcher.NavigationJournal.GetNavigationEntries(navigationType, metadata);
            return list.Where(entry => entry.NavigationType != NavigationType.Tab).OrderByDescending(entry => entry.NavigationDate).FirstOrDefault();
        }

        #endregion

        #region Nested types

        protected class NavigationContext : INavigationContext
        {
            #region Fields

            private IMetadataContext? _metadata;

            #endregion

            #region Constructors

            public NavigationContext(INavigationProvider navigationProvider, NavigationMode navigationMode, NavigationType navigationTypeFrom,
                IViewModelBase? viewModelFrom, NavigationType navigationTypeTo, IViewModelBase? viewModelTo, IReadOnlyMetadataContext? metadata)
            {
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
                    {
                        var metadataContext = new MetadataContext();
                        metadataContext.Merge(metadata);
                        _metadata = metadataContext;
                    }
                }
            }

            #endregion

            #region Properties

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        MugenExtensions.LazyInitialize(ref _metadata, new MetadataContext());
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