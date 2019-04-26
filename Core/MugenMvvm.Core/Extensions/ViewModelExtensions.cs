using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static bool TrySubscribe(this IViewModelBase viewModel, object observer, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null, IViewModelDispatcher? dispatcher = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            return dispatcher.ServiceIfNull().Subscribe(viewModel, observer, executionMode ?? ThreadExecutionMode.Current, metadata.DefaultIfNull());
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

        public static void InvalidateCommands<TViewModel>(this TViewModel viewModel) where TViewModel : class, IViewModelBase, IHasService<IEventPublisher>
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Service.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        public static bool IsDisposed(this IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return viewModel.Metadata.Get(ViewModelMetadata.LifecycleState).IsDispose;
        }

        #endregion
    }
}