using MugenMvvm.Enums;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Infrastructure.Navigation
{
    public sealed class ApplicationStateAwareNavigationListener : IApplicationStateDispatcherListener
    {
        #region Fields

        private readonly INavigationDispatcher _navigationDispatcher;

        #endregion

        #region Constructors

        public ApplicationStateAwareNavigationListener(INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            var entries = _navigationDispatcher.NavigationJournal.GetNavigationEntries(null, metadata);
            for (var i = 0; i < entries.Count; i++)
                OnApplicationStateChanged(entries[i], dispatcher, oldState, newState, metadata);
        }

        public int GetPriority(object source)
        {
            return Priority;
        }

        #endregion

        #region Methods

        private void OnApplicationStateChanged(INavigationEntry entry, IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState,
            IReadOnlyMetadataContext metadata)
        {
            if (!(entry.NavigationProvider is IApplicationStateAwareNavigationProvider navigationProvider))
                return;
            var viewModel = entry.ViewModel;
            if (viewModel == null)
                return;
            if (!navigationProvider.IsSupported(viewModel, oldState, newState, metadata))
                return;

            var ctx = navigationProvider.TryCreateApplicationStateContext(viewModel, oldState, newState, metadata);
            if (ctx == null)
            {
                if (newState == ApplicationState.Active)
                {
                    ctx = _navigationDispatcher.ContextFactory.GetNavigationContext(navigationProvider, NavigationMode.Foreground, entry.NavigationType, entry.ViewModel,
                        NavigationType.System, null, metadata);
                }
                else if (newState == ApplicationState.Background)
                {
                    ctx = _navigationDispatcher.ContextFactory.GetNavigationContext(navigationProvider, NavigationMode.Background, NavigationType.System, null,
                        entry.NavigationType, entry.ViewModel, metadata);
                }
            }

            if (ctx != null)
                _navigationDispatcher.OnNavigated(ctx);
        }

        #endregion
    }
}