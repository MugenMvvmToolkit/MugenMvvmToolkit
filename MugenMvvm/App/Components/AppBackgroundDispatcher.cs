using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;

namespace MugenMvvm.App.Components
{
    public sealed class AppBackgroundDispatcher : IApplicationLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private INavigationContext? _backgroundCloseContext;
        private INavigationContext? _backgroundNewContext;

        #endregion

        #region Constructors

        public AppBackgroundDispatcher(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = AppComponentPriority.BackgroundDispatcher;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            if (lifecycleState == ApplicationLifecycleState.Activating)
                dispatcher.OnNavigating(BackgroundCloseContext(application));
            else if (lifecycleState == ApplicationLifecycleState.Activated)
            {
                application.Metadata.Set(ApplicationMetadata.IsInBackground, false);
                var closeContext = BackgroundCloseContext(application);
                dispatcher
                    .GetComponents<INavigationCallbackManagerComponent>()
                    .TryAddNavigationCallback(dispatcher, NavigationCallbackType.Showing, InternalConstant.BackgroundNavigationId, NavigationType.Background, application, metadata);
                dispatcher.OnNavigated(closeContext);
                closeContext.ClearMetadata(true);
            }
            else if (lifecycleState == ApplicationLifecycleState.Deactivating)
                dispatcher.OnNavigating(BackgroundNewContext(application));
            else if (lifecycleState == ApplicationLifecycleState.Deactivated)
            {
                application.Metadata.Set(ApplicationMetadata.IsInBackground, true);
                var newContext = BackgroundNewContext(application);
                dispatcher
                    .GetComponents<INavigationCallbackManagerComponent>()
                    .TryAddNavigationCallback(dispatcher, NavigationCallbackType.Close, InternalConstant.BackgroundNavigationId, NavigationType.Background, application, metadata);
                dispatcher.OnNavigated(newContext);
                newContext.ClearMetadata(true);
            }
        }

        #endregion

        #region Methods

        private INavigationContext BackgroundNewContext(IMugenApplication application) =>
            _backgroundNewContext ??= new NavigationContext(application, Default.NavigationProvider, InternalConstant.BackgroundNavigationId, NavigationType.Background, NavigationMode.New);

        private INavigationContext BackgroundCloseContext(IMugenApplication application) =>
            _backgroundCloseContext ??= new NavigationContext(application, Default.NavigationProvider, InternalConstant.BackgroundNavigationId, NavigationType.Background, NavigationMode.Close);

        #endregion
    }
}