using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool TrySubscribe(this IViewModelBase viewModel, object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var service = viewModel.TryGetService<IMessenger>();
            return service != null && service.Subscribe(subscriber, executionMode, metadata);
        }

        public static bool TryUnsubscribe(this IViewModelBase viewModel, object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var service = viewModel.TryGetService<IMessenger>();
            return service != null && service.Unsubscribe(subscriber, metadata);
        }

        public static TService? TryGetService<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasService<TService> hasService)
                return hasService.Service;
            return null;
        }

        public static TService? TryGetServiceOptional<TService>(this IViewModelBase viewModel) where TService : class
        {
            if (viewModel is IHasServiceOptional<TService> hasServiceOptional)
                return hasServiceOptional.ServiceOptional;
            return viewModel.TryGetService<TService>();
        }

        public static IReadOnlyMetadataContext NotifyLifecycleChanged(this IViewModelBase viewModel, ViewModelLifecycleState state, IReadOnlyMetadataContext? metadata = null,
            IViewModelManager? manager = null)
        {
            return manager.DefaultIfNull().OnLifecycleChanged(viewModel, state, metadata);
        }

        public static void InvalidateCommands<TViewModel>(this TViewModel viewModel) where TViewModel : class, IViewModelBase, IHasService<IMessagePublisher>
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Service.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        public static bool IsDisposed(this IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return viewModel.Metadata.Get(ViewModelMetadata.LifecycleState, ViewModelLifecycleState.Created).IsDispose;
        }

        #endregion
    }
}