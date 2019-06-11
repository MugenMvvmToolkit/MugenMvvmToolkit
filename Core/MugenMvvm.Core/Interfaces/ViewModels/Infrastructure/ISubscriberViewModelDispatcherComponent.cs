using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface ISubscriberViewModelDispatcherComponent : IViewModelDispatcherComponent
    {
        bool TrySubscribe(IViewModelDispatcher dispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode,
            IReadOnlyMetadataContext metadata);

        bool TryUnsubscribe(IViewModelDispatcher dispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);
    }
}