using System.Diagnostics.CodeAnalysis;
using System.IO;
using Android.OS;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewStateDispatcher : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewStateDispatcher(IViewModelManager? viewModelManager = null, IPresenter? presenter = null, ISerializer? serializer = null)
        {
            _viewModelManager = viewModelManager;
            _presenter = presenter;
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.StateManager;

        public bool SaveState { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == AndroidViewLifecycleState.SavingState && view is IView v && TryGetBundle(view, state, false, out var bundle))
                PreserveState(v, bundle, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Creating && TryGetBundle(view, state, true, out var b))
            {
                view = MugenExtensions.GetUnderlyingView(view);
                var request = TryRestoreState(view, b, metadata);
                if (request == null)
                {
                    FragmentExtensions.ClearFragmentState(b);
                    if (view is IActivityView av)
                        Finish(av);
                    else if (view is IFragmentView f)
                        FragmentExtensions.Remove(f);
                }
                else if (_presenter.DefaultIfNull().TryShow(request, default, metadata).IsNullOrEmpty())
                {
                    if (view is IActivityView activity)
                        activity.Finish();
                    else
                        viewManager.TryInitializeAsync(ViewMapping.Undefined, request, default, metadata);
                }
                else
                    viewManager.OnLifecycleChanged(view, AndroidViewLifecycleState.PendingInitialization, state, metadata);
            }
        }

        #endregion

        #region Methods

        private void Finish(IActivityView activityView)
        {
            if (!ActivityExtensions.IsTaskRoot(activityView) || _presenter.DefaultIfNull().Show(activityView).IsNullOrEmpty())
                activityView.Finish();
        }

        private void PreserveState(IView view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            bundle.PutString(AndroidInternalConstant.BundleVmId, view.ViewModel.GetId());
            if (!SaveState)
                return;

            var state = ViewModelMetadata.ViewModel.ToContext(view.ViewModel);
            using var stream = new MemoryStream();
            if (_serializer.DefaultIfNull().TrySerialize(stream, state, metadata))
                bundle.PutByteArray(AndroidInternalConstant.BundleViewState, stream.ToArray());
            else
                bundle.Remove(AndroidInternalConstant.BundleViewState);
        }

        private ViewModelViewRequest? TryRestoreState(object view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            var id = bundle.GetString(AndroidInternalConstant.BundleVmId);
            if (string.IsNullOrEmpty(id))
                return null;

            var viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id!, metadata);
            if (viewModel == null)
            {
                if (!SaveState)
                    return null;

                var state = bundle.GetByteArray(AndroidInternalConstant.BundleViewState);
                if (state == null)
                    return null;

                using var stream = new MemoryStream(state);
                if (!_serializer.DefaultIfNull().TryDeserialize(stream, metadata, out var value) || !(value is IReadOnlyMetadataContext restoredState))
                    return null;

                viewModel = restoredState.Get(ViewModelMetadata.ViewModel);
                if (viewModel == null)
                    return null;
            }

            return new ViewModelViewRequest(viewModel, view);
        }

        private static bool TryGetBundle(object view, object? state, bool extrasAsFallback, [NotNullWhen(true)] out Bundle? bundle)
        {
            while (true)
            {
                if (state is Bundle b)
                {
                    bundle = b;
                    return true;
                }

                if (state is ICancelableRequest cancelableRequest)
                {
                    if (cancelableRequest.Cancel.GetValueOrDefault())
                    {
                        bundle = null;
                        return false;
                    }

                    state = cancelableRequest.State;
                    continue;
                }

                if (extrasAsFallback && MugenExtensions.GetUnderlyingView(view) is IActivityView activityView)
                {
                    bundle = ActivityExtensions.GetExtras(activityView);
                    return bundle != null;
                }

                bundle = null;
                return false;
            }
        }

        #endregion
    }
}