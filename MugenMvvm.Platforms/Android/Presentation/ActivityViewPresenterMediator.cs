using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Requests;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Presentation;

namespace MugenMvvm.Android.Presentation
{
    public class ActivityViewPresenterMediator : ViewPresenterMediatorBase<IActivityView>
    {
        private static int _counter;

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IPresenter? _presenter;

        public ActivityViewPresenterMediator(IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            _presenter = presenter;
            _navigationDispatcher = navigationDispatcher;
        }

        public override NavigationType NavigationType => NavigationType.Page;

        protected IPresenter Presenter => _presenter.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        protected virtual void NewActivity(IViewModelPresenterMediator mediator, INavigationContext navigationContext, int requestId)
        {
            var flags = navigationContext.GetOrDefault(NavigationMetadata.ClearBackStack) ? (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask) : 0;
            StartActivity(mediator, NavigationDispatcher.GetTopView<IActivityView>(NavigationType, true, mediator.ViewModel, navigationContext.GetMetadataOrDefault()), requestId,
                flags, navigationContext);
        }

        protected virtual bool IsTargetActivity(object view, ViewLifecycleState lifecycleState, object? state, IViewModelPresenterMediator mediator,
            INavigationContext navigationContext,
            int requestId, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState != AndroidViewLifecycleState.Created && lifecycleState != AndroidViewLifecycleState.Starting ||
                MugenExtensions.Unwrap(view) is not IActivityView activity)
                return false;

            return requestId == ActivityMugenExtensions.GetRequestId(activity);
        }

        protected virtual async Task RefreshActivityAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            var flags = 0;
            if (navigationContext.GetOrDefault(NavigationMetadata.ClearBackStack))
            {
                await NavigationDispatcher.ClearBackStackAsync(NavigationType, mediator.ViewModel, false, navigationContext.GetMetadataOrDefault(), Presenter, cancellationToken);
                flags = (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var topActivity = NavigationDispatcher.GetTopView<IActivityView>(NavigationType, true, null, navigationContext.GetMetadataOrDefault());
            if (Equals(topActivity, view))
                return;

            flags |= (int)ActivityFlags.ReorderToFront;
            StartActivity(mediator, topActivity, ActivityMugenExtensions.GetRequestId(view), flags, navigationContext);
        }

        protected virtual void StartActivity(IViewModelPresenterMediator mediator, IActivityView? topActivity, int requestId, int flags, INavigationContext navigationContext)
        {
            var mapping = mediator.Mapping;
            Class? activityType = null;
            var resourceId = 0;
            if (!mapping.ViewType.IsInterface && typeof(Object).IsAssignableFrom(mapping.ViewType))
                activityType = Class.FromType(mapping.ViewType);
            if (mapping is IResourceViewMapping m)
                resourceId = m.ResourceId;

            if (!ActivityMugenExtensions.StartActivity(topActivity!, activityType!, requestId, mediator.ViewModel.GetId(), resourceId, flags))
                ExceptionManager.ThrowPresenterCannotShowRequest(mediator.Mapping, navigationContext.GetMetadataOrDefault());
        }

        protected override object? TryGetViewRequest(IViewModelPresenterMediator mediator, IActivityView? view, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode == NavigationMode.New && view == null)
            {
                return new ActivityViewRequest<(ActivityViewPresenterMediator, IViewModelPresenterMediator, INavigationContext, int)>(mediator.ViewModel, mediator.Mapping,
                    state => state.Item1.NewActivity(state.Item2, state.Item3, state.Item4),
                    (v, l, s, state, m) => state.Item1.IsTargetActivity(v, l, s, state.Item2, state.Item3, state.Item4, m),
                    (this, mediator, navigationContext, Interlocked.Increment(ref _counter)));
            }

            return null;
        }

        protected override Task ActivateAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var topActivityView = NavigationDispatcher.GetTopView<IActivityView>(NavigationType, true, null, navigationContext.GetMetadataOrDefault());
            if (Equals(topActivityView, view))
                return Task.CompletedTask;
            return RefreshActivityAsync(mediator, view, navigationContext, cancellationToken);
        }

        protected override Task ShowAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        protected override Task CloseAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            view.Finish();
            return Task.CompletedTask;
        }
    }
}