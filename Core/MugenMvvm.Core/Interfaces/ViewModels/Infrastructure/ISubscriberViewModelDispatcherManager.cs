using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface ISubscriberViewModelDispatcherManager : IViewModelDispatcherManager
    {
        bool TrySubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        bool TryUnsubscribe(IViewModelDispatcher viewModelDispatcher, IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata);
    }
}