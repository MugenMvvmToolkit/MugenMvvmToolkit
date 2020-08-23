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
    public sealed class AndroidViewInitializer : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;

        #endregion

        #region Constructors

        public AndroidViewInitializer(IPresenter? presenter = null)
        {
            _presenter = presenter;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer - 1;

        public bool FinishNotInitializedView { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == AndroidViewLifecycleState.Started && !(view is IView) && viewManager.GetViews(view, metadata).IsNullOrEmpty())
                _presenter.DefaultIfNull().TryShow(view, default, metadata);
            else if (FinishNotInitializedView && lifecycleState == AndroidViewLifecycleState.Resumed && view is IActivityView activityView && viewManager.GetViews(view).IsNullOrEmpty())
                activityView.Finish();
        }

        #endregion
    }
}