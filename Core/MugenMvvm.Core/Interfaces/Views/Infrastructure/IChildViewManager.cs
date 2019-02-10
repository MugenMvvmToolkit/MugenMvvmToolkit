using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IChildViewManager
    {
        int Priority { get; }

        IReadOnlyList<IViewInfo> GetViews(IParentViewManager parentViewManager, IViewModel viewModel, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(IParentViewManager parentViewManager, object view, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IParentViewManager parentViewManager, IViewModel viewModel, IReadOnlyMetadataContext metadata);

        IViewManagerResult<IViewInfo>? TryInitialize(IParentViewManager parentViewManager, IViewModel viewModel, object view, IReadOnlyMetadataContext metadata);
    }
}