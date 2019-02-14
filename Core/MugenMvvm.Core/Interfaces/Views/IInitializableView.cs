using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Interfaces.Views
{
    public interface IInitializableView : IView
    {
        void Initialize(IViewModelBase viewModel, IViewInfo viewInfo, IReadOnlyMetadataContext metadata);
    }
}