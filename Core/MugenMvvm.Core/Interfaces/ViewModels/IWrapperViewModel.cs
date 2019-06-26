using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IWrapperViewModel : IViewModelBase //todo check usages, use as extension, todo add presenter
    {
        IViewModelBase ViewModel { get; }

        void Wrap(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null);
    }
}