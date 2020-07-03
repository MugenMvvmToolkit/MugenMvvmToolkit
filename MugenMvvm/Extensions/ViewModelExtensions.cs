using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static object GetService<TRequest>(this IViewModelManager viewModelManager, IViewModelBase viewModel, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModelManager, nameof(viewModelManager));
            var result = viewModelManager.TryGetService(viewModel, request, metadata);
            if (result == null)
                ExceptionManager.ThrowCannotResolveService(request);
            return result;
        }

        public static bool TrySubscribe<T>(this IViewModelBase viewModel, [DisallowNull] in T subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var service = viewModel.TryGetService<IMessenger>();
            return service != null && service.TrySubscribe(subscriber, executionMode, metadata);
        }

        public static bool TryUnsubscribe<T>(this IViewModelBase viewModel, [DisallowNull] in T subscriber, IReadOnlyMetadataContext? metadata = null)
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

        public static void NotifyLifecycleChanged<TState>(this IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, in TState state,
            IReadOnlyMetadataContext? metadata = null, IViewModelManager? manager = null)
        {
            manager.DefaultIfNull().OnLifecycleChanged(viewModel, lifecycleState, state, metadata);
        }

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