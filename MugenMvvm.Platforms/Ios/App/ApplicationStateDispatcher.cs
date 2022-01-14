using System;
using Foundation;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Ios.Constants;
using MugenMvvm.Ios.Enums;
using MugenMvvm.Ios.Extensions;
using MugenMvvm.Ios.Requests;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Views;
using UIKit;

namespace MugenMvvm.Ios.App
{
    public sealed class ApplicationStateDispatcher : IApplicationLifecycleListener, IViewLifecycleListener, IHasPriority
    {
        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewManager? _viewManager;

        public ApplicationStateDispatcher(IPresenter? presenter = null, ISerializer? serializer = null, IViewManager? viewManager = null, IServiceProvider? serviceProvider = null)
        {
            _presenter = presenter;
            _serializer = serializer;
            _viewManager = viewManager;
            _serviceProvider = serviceProvider;
        }

        public int Priority { get; init; } = ViewComponentPriority.StateManager;

        public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == IosApplicationLifecycleState.Preserving && state is ICancelableRequest cancelableRequest
                                                                          && cancelableRequest.Cancel == null)
            {
                cancelableRequest.Cancel = UIApplication.SharedApplication.Delegate.GetWindow()?.RootViewController == null ||
                                           !_serializer.DefaultIfNull().IsSupported(SerializationFormat.AppStateBytes);
            }
            else if (lifecycleState == IosApplicationLifecycleState.Preserved && state is NSCoder coder)
            {
                var rootViewController = UIApplication.SharedApplication.Delegate.GetWindow()?.RootViewController;
                if (rootViewController != null)
                {
                    rootViewController.RestorationIdentifier ??= IosInternalConstants.RootViewControllerId;
                    coder.Encode(application.PlatformInfo.ApplicationVersion, IosInternalConstants.AppVersionKey);
                    coder.Encode(rootViewController, IosInternalConstants.RootViewControllerKey);
                }
            }
            else if (lifecycleState == IosApplicationLifecycleState.Restoring && state is ICancelableRequest cancelable
                                                                              && cancelable.Cancel == null && cancelable.State is NSCoder c)
                cancelable.Cancel = c.DecodeString(IosInternalConstants.AppVersionKey) != application.PlatformInfo.ApplicationVersion;
            else if (lifecycleState == IosApplicationLifecycleState.RestoringViewController && state is RestoreViewControllerRequest request)
            {
                var bytes = request.Coder.DecodeBytes(IosInternalConstants.ViewModelStateKey);
                var typeSt = request.Coder.DecodeString(IosInternalConstants.ViewControllerTypeKey);
                var viewType = typeSt == null ? null : Type.GetType(typeSt, false);
                if (viewType != null && bytes != null)
                {
                    IReadOnlyMetadataContext? restoredState = null;
                    if (_serializer.DefaultIfNull().TryDeserialize(DeserializationFormat.AppStateBytes, bytes, ref restoredState, metadata)
                        && restoredState.TryGet(ViewModelMetadata.ViewModel, out var vm) && vm != null)
                    {
                        var viewModelViewRequest = new ViewModelViewRequest(vm, viewType);
                        var view = _viewManager.DefaultIfNull().TryInitializeAsync(ViewMapping.Undefined, viewModelViewRequest, default, metadata).GetResult();
                        if (view != null)
                        {
                            request.ViewController = (UIViewController) view.Target;
                            request.ViewController.RestorationIdentifier = request.RestorationIdentifier;
                            _presenter.DefaultIfNull().TryShow(ViewModelViewRequest.GetRequestOrRaw(viewModelViewRequest, vm, view.Target), default, metadata);
                        }
                    }
                }
                else if (viewType != null && request.RestorationIdentifier == IosInternalConstants.RootViewControllerId)
                {
                    var controller = (UIViewController?) _serviceProvider.DefaultIfNull().GetService(viewType, metadata);
                    if (controller != null && !_presenter.DefaultIfNull().TryShow(controller, default, metadata).IsEmpty)
                    {
                        controller.RestorationIdentifier = IosInternalConstants.RootViewControllerId;
                        request.ViewController = controller;
                    }
                }
            }
        }

        public void OnLifecycleChanged(IViewManager viewManager, ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState.BaseState == ViewLifecycleState.Initialized && view.TryGet<UIViewController>(out var viewController)
                                                                           && view.View != null && viewController.RestorationIdentifier == null)
                viewController.RestorationIdentifier = view.View.ViewModel.GetId();
            else if (lifecycleState == IosViewLifecycleState.EncodingRestorableState && state is NSCoder coder)
            {
                var serializer = _serializer.DefaultIfNull();
                if (!serializer.IsSupported(SerializationFormat.AppStateBytes))
                    return;

                if (view.View != null)
                {
                    var stateMeta = ViewModelMetadata.ViewModel.ToContext(view.View.ViewModel);
                    ReadOnlyMemory<byte> buffer = default;
                    if (serializer.TrySerialize(SerializationFormat.AppStateBytes, stateMeta, ref buffer, metadata))
                    {
                        coder.Encode(buffer.ToArray(), IosInternalConstants.ViewModelStateKey);
                        coder.Encode(view.View.Target.GetType().AssemblyQualifiedName, IosInternalConstants.ViewControllerTypeKey);
                    }
                }
                else if (view.TryGet<UIViewController>(out var vc) && vc.RestorationIdentifier == IosInternalConstants.RootViewControllerId)
                    coder.Encode(vc.GetType().AssemblyQualifiedName, IosInternalConstants.ViewControllerTypeKey);
            }
        }
    }
}