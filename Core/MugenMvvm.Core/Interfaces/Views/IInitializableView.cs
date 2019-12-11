using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IInitializableView
    {
        void Initialize(IViewModelBase viewModel, IView view, IReadOnlyMetadataContext? metadata);
    }
}