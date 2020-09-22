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
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Android.Presenters
{
    public class ActivityViewPresenter : ViewPresenterBase<IActivityView>
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;
        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public ActivityViewPresenter(IPresenter? presenter = null, INavigationDispatcher? navigationDispatcher = null)
        {
            _presenter = presenter;
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public override NavigationType NavigationType => NavigationType.Page;

        protected IPresenter Presenter => _presenter.DefaultIfNull();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher.DefaultIfNull();

        #endregion

        #region Methods

        protected override object? TryGetViewRequest(IViewModelPresenterMediator mediator, IActivityView? view, INavigationContext navigationContext)
        {
            if (navigationContext.NavigationMode == NavigationMode.New && view == null)
                return new ActivityViewRequest<(ActivityViewPresenter, IViewModelPresenterMediator, INavigationContext)>(mediator.ViewModel, mediator.Mapping,
                    state => state.Item1.NewActivity(state.Item2, state.Item3), (this, mediator, navigationContext));
            return null;
        }

        protected override Task? ActivateAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            var topActivityView = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (Equals(topActivityView, view))
                return null;
            return RefreshActivityAsync(mediator, view, navigationContext);
        }

        protected override Task? ShowAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext) => null;

        protected override Task? CloseAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            view.Finish();
            return null;
        }

        protected virtual void NewActivity(IViewModelPresenterMediator mediator, INavigationContext navigationContext)
        {
            var flags = navigationContext.GetMetadataOrDefault().Get(NavigationMetadata.ClearBackStack) ? (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask) : 0;
            StartActivity(mediator, NavigationDispatcher.GetTopView<IActivityView>(NavigationType), flags, null, navigationContext);
        }

        protected virtual async Task RefreshActivityAsync(IViewModelPresenterMediator mediator, IActivityView view, INavigationContext navigationContext)
        {
            var flags = 0;
            var metadata = navigationContext.GetMetadataOrDefault();
            if (metadata.Get(NavigationMetadata.ClearBackStack))
            {
                await NavigationDispatcher.ClearBackStackAsync(NavigationType, mediator.ViewModel, false, metadata, Presenter);
                flags = (int)(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            var topActivity = NavigationDispatcher.GetTopView<IActivityView>(NavigationType);
            if (Equals(topActivity, view))
                return;

            flags |= (int)ActivityFlags.ReorderToFront;
            var bundle = new Bundle(1);
            bundle.PutString(AndroidInternalConstant.BundleVmId, mediator.ViewModel.GetId());
            StartActivity(mediator, topActivity, flags, bundle, navigationContext);
        }

        protected virtual void StartActivity(IViewModelPresenterMediator mediator, IActivityView? topActivity, int flags, Bundle? bundle, INavigationContext navigationContext)
        {
            var mapping = mediator.Mapping;
            Class? activityType = null;
            var resourceId = 0;
            if (!mapping.ViewType.IsInterface && typeof(Object).IsAssignableFrom(mapping.ViewType))
                activityType = Class.FromType(mapping.ViewType);
            if (mapping is IResourceViewMapping m)
                resourceId = m.ResourceId;

            if (!ActivityExtensions.StartActivity(topActivity!, activityType!, resourceId, flags, bundle!))
                ExceptionManager.ThrowPresenterCannotShowRequest(mediator.Mapping, navigationContext.GetMetadataOrDefault());
        }

        #endregion
    }
}