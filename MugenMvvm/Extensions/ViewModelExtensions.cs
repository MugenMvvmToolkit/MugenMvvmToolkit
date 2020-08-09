using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Views;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IView GetOrCreateView(this IViewModelBase viewModel, Type viewType, IReadOnlyMetadataContext? metadata = null, IViewManager? viewManager = null)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            return viewManager.DefaultIfNull().InitializeAsync(ViewMapping.Undefined, new ViewModelViewRequest(viewModel, viewType), default, metadata).Result;
        }

        public static object GetService(this IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            var result = viewModelManager.TryGetService(viewModel, request, metadata);
            if (result == null)
                ExceptionManager.ThrowCannotResolveService(request);
            return result;
        }

        public static bool TrySubscribe(this IViewModelBase viewModel, object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var service = viewModel.TryGetService<IMessenger>();
            return service != null && service.TrySubscribe(subscriber, executionMode, metadata);
        }

        public static bool TryUnsubscribe(this IViewModelBase viewModel, object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var service = viewModel.TryGetService<IMessenger>();
            return service != null && service.TryUnsubscribe(subscriber, metadata);
        }

        public static TService? TryGetService<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasService<TService> hasService)
                return hasService.Service;
            if (viewModel is IHasOptionalService<TService> hasOptionalService)
                return hasOptionalService.Service;
            if (viewModel is IComponentOwner owner && owner.HasComponents)
                return owner.Components.Get<TService>().FirstOrDefault();
            return null;
        }

        public static TService? TryGetOptionalService<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasOptionalService<TService> hasOptionalService)
                return hasOptionalService.Service;
            return viewModel.TryGetService<TService>();
        }

        public static void NotifyLifecycleChanged(this IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state = null,
            IReadOnlyMetadataContext? metadata = null, IViewModelManager? manager = null) =>
            manager.DefaultIfNull().OnLifecycleChanged(viewModel, lifecycleState, state, metadata);

        public static void InvalidateCommands<TViewModel>(this TViewModel viewModel) where TViewModel : class, IViewModelBase, IHasService<IMessagePublisher>
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Service.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        public static bool IsDisposed(this IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var lifecycleState = viewModel.GetMetadataOrDefault().Get(ViewModelMetadata.LifecycleState, ViewModelLifecycleState.Created);
            return lifecycleState == ViewModelLifecycleState.Disposed || lifecycleState == ViewModelLifecycleState.Finalized;
        }

        #endregion
    }
}