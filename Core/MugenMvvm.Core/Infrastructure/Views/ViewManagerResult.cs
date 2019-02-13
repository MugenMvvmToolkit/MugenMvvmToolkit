using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManagerResult : IViewManagerResult
    {
        #region Constructors

        public ViewManagerResult(IViewModelBase viewModel, IViewInfo view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            ViewModel = viewModel;
            View = view;
            Metadata = metadata;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public IViewModelBase ViewModel { get; }

        public IViewInfo View { get; }

        #endregion
    }
}