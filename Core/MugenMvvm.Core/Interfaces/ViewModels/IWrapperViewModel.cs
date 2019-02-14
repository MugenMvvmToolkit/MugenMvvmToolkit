using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IWrapperViewModel : IViewModelBase//todo check usages, use as extension
    {
        IViewModelBase ViewModel { get; }

        void Wrap(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}