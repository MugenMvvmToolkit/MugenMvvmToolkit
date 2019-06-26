using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels.Components
{
    public interface ISubscriberViewModelDispatcherComponent : IComponent<IViewModelDispatcher>
    {
        bool TrySubscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext? metadata);
    }
}