using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewFirstInitializer : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public AndroidViewFirstInitializer(IPresenter? presenter = null)
        {
            _presenter = presenter;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer - 1;

        public bool FinishWithoutView { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (view is IView)
                return;

            var views = viewManager.GetViews(view, metadata);
            if (lifecycleState == AndroidViewLifecycleState.Started)
            {
                if (views.IsNullOrEmpty())
                    _presenter.DefaultIfNull().TryShow(view, default, metadata);
            }
            else if (lifecycleState == AndroidViewLifecycleState.Resumed && FinishWithoutView && view is IActivityView activityView)
                activityView.Finish();
        }

        #endregion
    }
}