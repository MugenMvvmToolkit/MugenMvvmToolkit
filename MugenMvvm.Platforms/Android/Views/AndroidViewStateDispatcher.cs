using System;
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
    public sealed class AndroidViewStateDispatcher : IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public AndroidViewStateDispatcher(IViewModelManager? viewModelManager = null, IPresenter? presenter = null, ISerializer? serializer = null)
        {
            _viewModelManager = viewModelManager;
            _presenter = presenter;
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.StateManager;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == AndroidViewLifecycleState.SavingState && view is IView v && state is ICancelableRequest cancelableRequest
                && !cancelableRequest.Cancel.GetValueOrDefault() && cancelableRequest.State is Bundle bundle)
                PreserveState(viewManager, v, bundle, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Creating && state is ICancelableRequest r && !r.Cancel.GetValueOrDefault() && r.State is Bundle b)
            {
                if (view is IView wrapperView)
                    view = wrapperView.Target;

                var request = TryRestoreState(viewManager, view, b, metadata);
                if (request == null)
                {
                    FragmentExtensions.ClearFragmentState(b);
                    if (view is IActivityView av)
                        av.Finish();
                    else if (view is IFragmentView f)
                        FragmentExtensions.Remove(f);
                    return;
                }

                viewManager.TryInitializeAsync(ViewMapping.Undefined, request, default, metadata);
                if (_presenter.DefaultIfNull().TryShow(request, default, metadata).IsNullOrEmpty() && view is IActivityView activity)
                    activity.Finish();
            }
        }

        #endregion

        #region Methods

        private void PreserveState(IViewManager viewManager, IView view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            var id = view.ViewModel.Metadata.Get(ViewModelMetadata.Id).ToString("N");
            bundle.PutString(AndroidInternalConstant.BundleVmId, id);

            var request = new StateRequest(null, view);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Preserving, request, metadata);
            if (request.Cancel.GetValueOrDefault() || !request.HasMetadata)
                bundle.Remove(AndroidInternalConstant.BundleViewState);
            else
            {
                using var stream = new MemoryStream();
                if (_serializer.DefaultIfNull().TrySerialize(stream, request, metadata))
                    bundle.Remove(AndroidInternalConstant.BundleViewState);
                else
                {
                    bundle.PutByteArray(AndroidInternalConstant.BundleViewState, stream.ToArray());
                    viewManager.OnLifecycleChanged(view, ViewLifecycleState.Preserved, request, metadata);
                }
            }
        }

        private ViewModelViewRequest? TryRestoreState(IViewManager viewManager, object view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            if (!Guid.TryParse(bundle.GetString(AndroidInternalConstant.BundleVmId), out var id))
                return null;

            var viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id, metadata);
            if (viewModel == null)
            {
                var state = bundle.GetByteArray(AndroidInternalConstant.BundleViewState);
                if (state == null)
                    return null;

                using var stream = new MemoryStream(state);
                if (!_serializer.DefaultIfNull().TryDeserialize(stream, metadata, out var value) || !(value is IReadOnlyMetadataContext restoredState))
                    return null;

                var request = new StateRequest(null, view, restoredState);
                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Restoring, request, metadata);
                if (request.Cancel.GetValueOrDefault())
                    return null;

                viewManager.OnLifecycleChanged(view, ViewLifecycleState.Restored, restoredState, metadata);
                viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id, metadata);

                if (viewModel == null)
                    return null;
            }

            return new ViewModelViewRequest(viewModel, view);
        }

        #endregion
    }
}