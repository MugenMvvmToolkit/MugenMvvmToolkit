using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool IsInState(this IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            return viewModelManager.GetComponents<ILifecycleTrackerComponent<ViewModelLifecycleState>>().IsInState(viewModelManager, viewModel, state, metadata);
        }

        public static IViewModelBase GetViewModel(this IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            Should.NotBeNull(request, nameof(request));
            var viewModel = viewModelManager.TryGetViewModel(request, metadata);
            if (viewModel == null)
                ExceptionManager.ThrowRequestNotSupported<IViewProviderComponent>(viewModelManager, request, metadata);
            return viewModel;
        }

        public static TViewModel GetViewModel<TViewModel>(this IViewModelManager viewModelManager, IReadOnlyMetadataContext? metadata = null)
            where TViewModel : class, IViewModelBase
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            return (TViewModel) viewModelManager.GetViewModel(typeof(TViewModel), metadata);
        }

        public static object GetService(this IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            var result = viewModelManager.TryGetService(viewModel, request, metadata);
            if (result == null)
                ExceptionManager.ThrowCannotResolveService(request);
            return result;
        }

        public static IView GetOrCreateView(this IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null) =>
            viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, viewModel, default, metadata).Result;

        public static IView GetOrCreateView(this IViewModelBase viewModel, Type viewType, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            return viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, new ViewModelViewRequest(viewModel, viewType), default, metadata).Result;
        }

        public static T InitializeService<TViewModel, T>(this TViewModel viewModel, ref T? service, object? request = null, Action<TViewModel, T>? callback = null, IReadOnlyMetadataContext? metadata = null,
            IViewModelManager? viewModelManager = null)
            where TViewModel : class, IViewModelBase
            where T : class
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (service == null)
            {
                lock (viewModel)
                {
                    if (service == null)
                    {
                        if (viewModel.IsInState(ViewModelLifecycleState.Disposed))
                            ExceptionManager.ThrowObjectDisposed(viewModel);
                        service = (T) viewModelManager.DefaultIfNull().GetService(viewModel, request ?? typeof(T), metadata);
                        callback?.Invoke(viewModel, service);
                    }
                }
            }

            return service;
        }

        public static string GetId(this IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return viewModel.Metadata.Get(ViewModelMetadata.Id)!;
        }

        public static bool IsInState(this IViewModelBase viewModel, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata = null, IViewModelManager? viewModelManager = null)
            => viewModelManager.DefaultIfNull().IsInState(viewModel, state, metadata);

        public static bool TrySubscribe(this IViewModelBase viewModel, object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var service = viewModel.TryGetService<IMessenger>(false);
            return service != null && service.TrySubscribe(subscriber, executionMode, metadata);
        }

        public static bool TryUnsubscribe(this IViewModelBase viewModel, object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var service = viewModel.TryGetService<IMessenger>(true);
            return service != null && service.TryUnsubscribe(subscriber, metadata);
        }

        public static TService? TryGetService<TService>(this IViewModelBase viewModel, bool optional) where TService : class
        {
            if (viewModel is IHasService<TService> hasService)
                return optional ? hasService.ServiceOptional : hasService.Service;
            return null;
        }

        public static void NotifyLifecycleChanged(this IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state = null,
            IReadOnlyMetadataContext? metadata = null, IViewModelManager? manager = null) =>
            manager.DefaultIfNull().OnLifecycleChanged(viewModel, lifecycleState, state, metadata);

        public static IViewModelBase? TryGetViewModelView<TView>(object request, out TView? view) where TView : class
        {
            if (request is ViewModelViewRequest viewModelViewRequest)
            {
                view = viewModelViewRequest.View as TView;
                return viewModelViewRequest.ViewModel;
            }

            if (request is IViewModelBase vm)
            {
                view = null;
                return vm;
            }

            if (request is IHasTarget<object?> hasTarget && hasTarget.Target is IViewModelBase result)
            {
                view = null;
                return result;
            }

            view = request as TView;
            return null;
        }

        public static void RegisterDisposeToken(this ViewModelBase viewModel, IDisposable token)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(token, nameof(token));
            viewModel.RegisterDisposeToken(new ActionToken((o, _) => ((IDisposable) o!).Dispose(), token));
        }

        #endregion
    }
}