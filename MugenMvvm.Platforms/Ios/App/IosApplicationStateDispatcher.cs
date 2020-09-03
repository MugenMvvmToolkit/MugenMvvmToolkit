using System;
using System.IO;
using Foundation;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
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
    public sealed class IosApplicationStateDispatcher : IApplicationLifecycleDispatcherComponent, IViewLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IServiceProvider? _serviceProvider;
        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public IosApplicationStateDispatcher(IPresenter? presenter = null, ISerializer? serializer = null, IViewManager? viewManager = null, IServiceProvider? serviceProvider = null)
        {
            _presenter = presenter;
            _serializer = serializer;
            _viewManager = viewManager;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.StateManager;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IMugenApplication application, ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == IosApplicationLifecycleState.Preserving && state is ICancelableRequest cancelableRequest
                                                                          && cancelableRequest.Cancel == null)
                cancelableRequest.Cancel = UIApplication.SharedApplication.Delegate.GetWindow()?.RootViewController == null;
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
                    using var stream = new MemoryStream(bytes);
                    if (_serializer.DefaultIfNull().TryDeserialize(stream, metadata, out var value)
                        && value is IReadOnlyMetadataContext restoredState && restoredState.TryGet(ViewModelMetadata.ViewModel, out var vm) && vm != null)
                    {
                        var viewModelViewRequest = new ViewModelViewRequest(vm, viewType);
                        var view = _viewManager.DefaultIfNull().TryInitializeAsync(ViewMapping.Undefined, viewModelViewRequest, default, metadata)?.Result;
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
                    var controller = (UIViewController?) _serviceProvider.DefaultIfNull().GetService(viewType);
                    if (controller != null && !_presenter.DefaultIfNull().TryShow(controller, default, metadata).IsNullOrEmpty())
                    {
                        controller.RestorationIdentifier = IosInternalConstants.RootViewControllerId;
                        request.ViewController = controller;
                    }
                }
            }
        }

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewLifecycleState.Initialized && MugenExtensions.GetUnderlyingView(view) is UIViewController viewController
                                                                 && viewController.RestorationIdentifier == null)
                viewController.RestorationIdentifier = Guid.NewGuid().ToString("N");
            else if (lifecycleState == IosViewLifecycleState.EncodingRestorableState && state is NSCoder coder)
            {
                if (view is IView v)
                {
                    var stateMeta = ViewModelMetadata.ViewModel.ToContext(v.ViewModel);
                    using var stream = new MemoryStream();
                    if (_serializer.DefaultIfNull().TrySerialize(stream, stateMeta, metadata))
                    {
                        coder.Encode(stream.ToArray(), IosInternalConstants.ViewModelStateKey);
                        coder.Encode(v.Target.GetType().AssemblyQualifiedName, IosInternalConstants.ViewControllerTypeKey);
                    }
                }
                else if (view is UIViewController vc && vc.RestorationIdentifier == IosInternalConstants.RootViewControllerId)
                    coder.Encode(vc.GetType().AssemblyQualifiedName, IosInternalConstants.ViewControllerTypeKey);
            }
        }

        #endregion
    }
}