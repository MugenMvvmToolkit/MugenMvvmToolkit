﻿using System;
using System.IO;
using Android.OS;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Components;
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
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidViewStateDispatcher : AttachableComponentBase<IViewManager>, IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;
        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public AndroidViewStateDispatcher(IViewModelManager? viewModelManager = null, IPresenter? presenter = null, ISerializer? serializer = null, IMetadataContextManager? metadataContextManager = null)
        {
            _viewModelManager = viewModelManager;
            _presenter = presenter;
            _metadataContextManager = metadataContextManager;
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TState>())
                return;

            if (lifecycleState == AndroidViewLifecycleState.SavingState && view is IView v && state is ICancelableRequest cancelableRequest
                && !cancelableRequest.Cancel && cancelableRequest.State is Bundle bundle)
                PreserveState(v, bundle, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Created && state is Bundle b)
            {
                if (view is IView wrapperView)
                    view = wrapperView.Target;

                if (!RestoreState(view, b, metadata))
                    (view as IActivityView)?.Finish();
            }
        }

        #endregion

        #region Methods

        private void PreserveState(IView view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            var id = view.ViewModel.Metadata.Get(ViewModelMetadata.Id).ToString("N");
            bundle.PutString(AndroidInternalConstant.BundleVmId, id);

            var request = new StateRequest(false, view, null, _metadataContextManager);
            Owner.OnLifecycleChanged(view, ViewLifecycleState.Preserving, request, metadata);
            if (request.Cancel || !request.HasMetadata)
                bundle.Remove(AndroidInternalConstant.BundleViewState);
            else
            {
                using var stream = new MemoryStream();
                if (_serializer.DefaultIfNull().TrySerialize(stream, request, metadata))
                    bundle.Remove(AndroidInternalConstant.BundleViewState);
                else
                {
                    bundle.PutByteArray(AndroidInternalConstant.BundleViewState, stream.ToArray());
                    Owner.OnLifecycleChanged(view, ViewLifecycleState.Preserved, request, metadata);
                }
            }
        }

        private bool RestoreState(object view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            if (!Guid.TryParse(bundle.GetString(AndroidInternalConstant.BundleVmId), out var id))
                return false;

            var viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id, metadata);
            if (viewModel == null)
            {
                var state = bundle.GetByteArray(AndroidInternalConstant.BundleViewState);
                if (state == null)
                    return false;

                using var stream = new MemoryStream(state);
                if (!_serializer.DefaultIfNull().TryDeserialize(stream, metadata, out var value) || !(value is IReadOnlyMetadataContext restoredState))
                    return false;

                var request = new StateRequest(false, view, restoredState, _metadataContextManager);
                Owner.OnLifecycleChanged(view, ViewLifecycleState.Restoring, request, metadata);
                if (request.Cancel)
                    return false;

                Owner.OnLifecycleChanged(view, ViewLifecycleState.Restored, restoredState, metadata);
                viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id, metadata);

                if (viewModel == null)
                    return false;
            }

            return !_presenter.DefaultIfNull().TryShow(new ViewModelViewRequest(viewModel, view), default, metadata).IsNullOrEmpty();
        }

        #endregion
    }
}