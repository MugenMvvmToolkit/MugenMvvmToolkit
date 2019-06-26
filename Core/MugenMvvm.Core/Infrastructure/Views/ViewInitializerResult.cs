using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Infrastructure.Views
{
    public sealed class ViewInitializerResult : IViewInitializerResult
    {
        #region Constructors

        public ViewInitializerResult(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            ViewInfo = viewInfo;
            ViewModel = viewModel;
            Metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => true;

        public IReadOnlyMetadataContext Metadata { get; }

        public IViewModelBase ViewModel { get; }

        public IViewInfo ViewInfo { get; }

        #endregion
    }
}