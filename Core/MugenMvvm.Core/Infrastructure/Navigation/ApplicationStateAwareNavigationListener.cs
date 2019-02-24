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

        public ApplicationStateAwareNavigationListener(IApplicationStateDispatcher applicationStateDispatcher, INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(applicationStateDispatcher, nameof(applicationStateDispatcher));
            _navigationDispatcher = navigationDispatcher;
            applicationStateDispatcher.AddListener(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnStateChanged(IApplicationStateDispatcher dispatcher, ApplicationState oldState, ApplicationState newState, IReadOnlyMetadataContext metadata)
        {
            var entries = _navigationDispatcher.GetNavigationEntries(null, metadata);
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
                    ctx = new NavigationContext(navigationProvider, entry.NavigationType, NavigationMode.Foreground, null, viewModel, metadata);
                else if (newState == ApplicationState.Background)
                    ctx = new NavigationContext(navigationProvider, entry.NavigationType, NavigationMode.Background, viewModel, null, metadata);
            }

            if (ctx != null)
                _navigationDispatcher.OnNavigated(ctx);
        }

        #endregion
    }
}