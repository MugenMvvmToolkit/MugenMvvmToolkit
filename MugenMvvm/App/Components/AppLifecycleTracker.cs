using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Navigation;

namespace MugenMvvm.App.Components
{
    public sealed class AppLifecycleTracker : LifecycleTrackerBase<ApplicationLifecycleState, IMugenApplication>, IApplicationLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IMessenger? _messenger;
        private readonly INavigationDispatcher? _navigationDispatcher;
        private INavigationContext? _backgroundCloseContext;
        private INavigationContext? _backgroundNewContext;

        #endregion

        #region Constructors

        public AppLifecycleTracker(INavigationDispatcher? navigationDispatcher = null, IMessenger? messenger = null)
        {
            _navigationDispatcher = navigationDispatcher;
            _messenger = messenger;
            Trackers.Add(TrackAppState);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = AppComponentPriority.BackgroundDispatcher;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged(application, lifecycleState, metadata);
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            if (lifecycleState == ApplicationLifecycleState.Activating)
                dispatcher.OnNavigating(BackgroundCloseContext(application));
            else if (lifecycleState == ApplicationLifecycleState.Activated)
            {
                var closeContext = BackgroundCloseContext(application);
                dispatcher
                    .GetComponents<INavigationCallbackManagerComponent>(metadata)
                    .TryAddNavigationCallback(dispatcher, NavigationCallbackType.Showing, InternalConstant.BackgroundNavigationId, NavigationType.Background, application, metadata);
                dispatcher.OnNavigated(closeContext);
                closeContext.ClearMetadata(true);
            }
            else if (lifecycleState == ApplicationLifecycleState.Deactivating)
                dispatcher.OnNavigating(BackgroundNewContext(application));
            else if (lifecycleState == ApplicationLifecycleState.Deactivated)
            {
                var newContext = BackgroundNewContext(application);
                dispatcher
                    .GetComponents<INavigationCallbackManagerComponent>(metadata)
                    .TryAddNavigationCallback(dispatcher, NavigationCallbackType.Close, InternalConstant.BackgroundNavigationId, NavigationType.Background, application, metadata);
                dispatcher.OnNavigated(newContext);
                newContext.ClearMetadata(true);
            }

            _messenger.DefaultIfNull().Publish(application, lifecycleState, metadata);
        }

        #endregion

        #region Methods

        private static void TrackAppState(IMugenApplication app, HashSet<ApplicationLifecycleState> states, ApplicationLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state == ApplicationLifecycleState.Activating || state == ApplicationLifecycleState.Activated ||
                state == ApplicationLifecycleState.Deactivating || state == ApplicationLifecycleState.Deactivated)
            {
                states.Remove(ApplicationLifecycleState.Deactivating);
                states.Remove(ApplicationLifecycleState.Deactivated);
                states.Remove(ApplicationLifecycleState.Activating);
                states.Remove(ApplicationLifecycleState.Activated);
            }
            else if (state == ApplicationLifecycleState.Initialized)
                states.Remove(ApplicationLifecycleState.Initializing);

            states.Add(state);
        }

        private INavigationContext BackgroundNewContext(IMugenApplication application) =>
            _backgroundNewContext ??= new NavigationContext(application, Default.NavigationProvider, InternalConstant.BackgroundNavigationId, NavigationType.Background, NavigationMode.New);

        private INavigationContext BackgroundCloseContext(IMugenApplication application) =>
            _backgroundCloseContext ??= new NavigationContext(application, Default.NavigationProvider, InternalConstant.BackgroundNavigationId, NavigationType.Background, NavigationMode.Close);

        #endregion
    }
}