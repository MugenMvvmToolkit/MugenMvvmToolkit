using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewLifecycleAwareViewModel : IViewModelBase
    {
        void OnViewLifecycleChanged(IView view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata);
    }
}