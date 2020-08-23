using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Java.Lang;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Requests;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class ActivityViewPresenter : ViewPresenterBase<IActivityView>
    {
        #region Fields

        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public ActivityViewPresenter(IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null) : base(navigationDispatcher)
        {
            _presenter = presenter;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        protected IPresenter Presenter => _presenter.DefaultIfNull();

        #endregion

        #region Methods

        public override Task WaitBeforeCloseAsync(IViewModelPresenterMediator mediator, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(mediator.ViewModel, this,
                (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close, true, metadata);

        protected override Task WaitBeforeShowAsync(IViewModelPresenterMediator mediator, IActivityView? view, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => NavigationDispatcher.WaitNavigationAsync(mediator.ViewModel, this,
                (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close ||
                                     callback.NavigationType == state.NavigationType &&
                                     (callback.CallbackType == NavigationCallbackType.Showing || callback.CallbackType == NavigationCallbackType.Closing), true, metadata);

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
            var flags = navigationContext.GetMetadataOrDefault().Get(NavigationMetadata.ClearBackStack) ? (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask) : 0;
            StartActivity(mediator, NavigationDispatcher.GetTopView<IActivityView>(NavigationType), flags, null, navigationContext);
        }

        protected virtual void RefreshActivity(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            var flags = 0;
            var metadata = navigationContext.GetMetadataOrDefault();
            if (metadata.Get(NavigationMetadata.ClearBackStack))
            {
                var task = NavigationDispatcher.ClearBackStackAsync(NavigationType, mediator.ViewModel, false, metadata, Presenter);
                if (!task.IsCompleted)
                {
                    task.ContinueWithEx((this, mediator, view, navigationContext), (_, state) => state.Item1.RefreshActivity(state.mediator, state.view, state.navigationContext));
                    return;
                }

                flags = (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            var topActivity = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (Equals(topActivity, view))
                return;

            flags |= (int)ActivityFlags.ReorderToFront;
            var bundle = new Bundle(1);
            bundle.PutString(AndroidInternalConstant.BundleVmId, mediator.ViewModel.Metadata.Get(ViewModelMetadata.Id).ToString("N"));
            StartActivity(mediator, topActivity, flags, bundle, navigationContext);
        }

        protected virtual void StartActivity(IViewModelPresenterMediator mediator, IActivityView? topActivity, int flags, Bundle? bundle, INavigationContext navigationContext)
        {
            var mapping = mediator.Mapping;
            Class? activityType = null;
            var resourceId = 0;
            if (!mapping.ViewType.IsInterface && typeof(Object).IsAssignableFrom(mapping.ViewType))
                activityType = Class.FromType(mapping.ViewType);
            if (mapping is IAndroidViewMapping m)
                resourceId = m.ResourceId;

            if (!ActivityExtensions.StartActivity(topActivity!, activityType!, resourceId, flags, bundle!))
                ExceptionManager.ThrowPresenterCannotShowRequest(mediator.Mapping, navigationContext.GetMetadataOrDefault());
        }

        #endregion
    }
}