using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewFirstInitializer : AttachableComponentBase<IViewManager>, IViewLifecycleDispatcherComponent, IHasPriority
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

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!(view is IView))
            {
                var views = Owner.GetViews(view, metadata);
                if (lifecycleState == AndroidViewLifecycleState.Started)
                {
                    if (views.IsNullOrEmpty())
                        _presenter.DefaultIfNull().TryShow(new ViewModelViewRequest(null, view), default, metadata);
                }
                else if (lifecycleState == AndroidViewLifecycleState.Resumed && FinishWithoutView && view is IActivityView activityView)
                    activityView.Finish();
            }
        }

        #endregion
    }
}