using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal.Components;
using MugenMvvm.Navigation;

namespace MugenMvvm.App.Components
{
    public sealed class AppLifecycleTracker : LifecycleTrackerBase<IMugenApplication, ApplicationLifecycleState, IMugenApplication>, IApplicationLifecycleListener, IHasPriority
    {
        private readonly IMessenger? _messenger;
        private readonly INavigationDispatcher? _navigationDispatcher;
        private INavigationContext? _backgroundCloseContext;
        private INavigationContext? _backgroundNewContext;

        public AppLifecycleTracker(INavigationDispatcher? navigationDispatcher = null, IMessenger? messenger = null, IAttachedValueManager? attachedValueManager = null)
            : base(attachedValueManager)
        {
            _navigationDispatcher = navigationDispatcher;
            _messenger = messenger;
            Trackers.Add(TrackAppState);
        }

        public int Priority { get; init; } = AppComponentPriority.LifecycleTracker;

        public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            OnLifecycleChanged(application, lifecycleState, metadata);
            var dispatcher = _navigationDispatcher.DefaultIfNull();
            if (lifecycleState.BaseState == ApplicationLifecycleState.Activating)
                dispatcher.OnNavigating(BackgroundCloseContext(application));
            else if (lifecycleState.BaseState == ApplicationLifecycleState.Activated)
            {
                var closeContext = BackgroundCloseContext(application);
                dispatcher
                    .GetComponents<INavigationCallbackManagerComponent>(metadata)
                    .TryAddNavigationCallback(dispatcher, NavigationCallbackType.Show, InternalConstant.BackgroundNavigationId, NavigationType.Background, application,
                        metadata);
                dispatcher.OnNavigated(closeContext);
                closeContext.ClearMetadata(true);
            }
            else if (lifecycleState.BaseState == ApplicationLifecycleState.Deactivating)
                dispatcher.OnNavigating(BackgroundNewContext(application));
            else if (lifecycleState.BaseState == ApplicationLifecycleState.Deactivated)
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

        private static void TrackAppState(IMugenApplication app, HashSet<ApplicationLifecycleState> states, ApplicationLifecycleState state, IReadOnlyMetadataContext? metadata)
        {
            if (state.BaseState == ApplicationLifecycleState.Activating || state.BaseState == ApplicationLifecycleState.Activated ||
                state.BaseState == ApplicationLifecycleState.Deactivating || state.BaseState == ApplicationLifecycleState.Deactivated)
            {
                states.Remove(ApplicationLifecycleState.Deactivating);
                states.Remove(ApplicationLifecycleState.Deactivated);
                states.Remove(ApplicationLifecycleState.Activating);
                states.Remove(ApplicationLifecycleState.Activated);
            }
            else if (state.BaseState == ApplicationLifecycleState.Initialized)
                states.Remove(ApplicationLifecycleState.Initializing);

            states.Add(state);
        }

        private INavigationContext BackgroundNewContext(IMugenApplication application) =>
            _backgroundNewContext ??= _navigationDispatcher.DefaultIfNull().GetNavigationContext(application, NavigationProvider.System, InternalConstant.BackgroundNavigationId,
                NavigationType.Background, NavigationMode.New);

        private INavigationContext BackgroundCloseContext(IMugenApplication application) =>
            _backgroundCloseContext ??= _navigationDispatcher.DefaultIfNull().GetNavigationContext(application, NavigationProvider.System, InternalConstant.BackgroundNavigationId,
                NavigationType.Background, NavigationMode.Close);
    }
}