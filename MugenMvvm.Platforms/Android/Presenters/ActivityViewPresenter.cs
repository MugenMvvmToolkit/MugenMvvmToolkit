using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Requests;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class ActivityViewPresenter : ViewPresenterBase<IActivityView>
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public ActivityViewPresenter(IViewManager? viewManager = null, INavigationDispatcher? navigationDispatcher = null) : base(navigationDispatcher)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        protected IViewManager ViewManager => _viewManager.DefaultIfNull();

        #endregion

        #region Methods

        public override Task WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close, metadata);

        protected override Task WaitBeforeShowAsync(IViewModelPresenterMediator mediator, IActivityView? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(this, (callback, state) => callback.NavigationType == state.NavigationType && callback.CallbackType == NavigationCallbackType.Showing, metadata);

        protected override object? TryGetViewRequest(IViewModelPresenterMediator mediator, IActivityView? view, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode == NavigationMode.New && view == null)
                return new AndroidActivityViewRequest(mediator.ViewModel, mediator.Mapping, () => NewActivity(mediator, navigationContext));
            return base.TryGetViewRequest(mediator, view, navigationContext);
        }

        protected override void Activate(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            var topActivityView = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (!Equals(topActivityView, view))
                RefreshActivity(mediator, view, navigationContext);
        }

        protected override void Show(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
        }

        protected override void Close(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext) => view.Finish();

        protected virtual void NewActivity(IViewModelPresenterMediator mediator, INavigationContext navigationContext)
        {
            var flags = 0;
            if (navigationContext.GetMetadataOrDefault().Get(NavigationMetadata.ClearBackStack))
                flags = (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask);

            StartActivity(mediator, NavigationDispatcher.GetTopView<IActivityView>(NavigationType), flags, navigationContext);
        }

        protected virtual void RefreshActivity(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            var flags = 0;
            var metadata = navigationContext.GetMetadataOrDefault();
            if (metadata.Get(NavigationMetadata.ClearBackStack))
            {
                var callbacks = ItemOrListEditor.Get<Task>();
                foreach (var navigationEntry in NavigationDispatcher.GetNavigationEntries(metadata).Iterator())
                {
                    if (navigationEntry.NavigationType != NavigationType || !(navigationEntry.Target is IActivityView targetActivity) || Equals(targetActivity, view))
                        continue;
                    foreach (var navigationCallback in NavigationDispatcher.GetNavigationCallbacks(navigationEntry, metadata).Iterator())
                    {
                        if (navigationCallback.CallbackType == NavigationCallbackType.Close)
                            callbacks.Add(navigationCallback.AsTask());
                    }

                    ViewManager.OnLifecycleChanged(targetActivity, ViewLifecycleState.Closed, null, metadata);
                    targetActivity.Finish();
                }

                var task = callbacks.ToItemOrList().WhenAll();
                if (!task.IsCompleted)
                {
                    task.ContinueWithEx((this, mediator, view, navigationContext), (t, tuple) => tuple.Item1.RefreshActivity(tuple.mediator, tuple.view, tuple.navigationContext));
                    return;
                }

                flags = (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            var topActivity = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (Equals(topActivity, view))
                return;

            flags |= (int)ActivityFlags.ReorderToFront;
            StartActivity(mediator, topActivity, flags, navigationContext);
        }

        protected virtual void StartActivity(IViewModelPresenterMediator mediator, IActivityView? topActivity, int flags, INavigationContext navigationContext)
        {
            var mapping = mediator.Mapping;
            Class? activityType = null;
            var resourceId = 0;
            if (!mapping.ViewType.IsInterface && typeof(Object).IsAssignableFrom(mapping.ViewType))
                activityType = Class.FromType(mapping.ViewType);
            if (mapping is IAndroidViewMapping m)
                resourceId = m.ResourceId;

            if (!ActivityExtensions.StartActivity(topActivity!, activityType!, resourceId, flags))
                ExceptionManager.ThrowPresenterCannotShowRequest(mediator.Mapping, navigationContext.GetMetadataOrDefault());
        }

        #endregion
    }
}