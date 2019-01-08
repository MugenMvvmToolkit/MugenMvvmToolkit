using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.ViewModels.Infrastructure
{
    public interface IViewModelDispatcherListener
    {
        void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata);
    }
}
