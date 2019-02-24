using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IChildViewManager : IHasPriority
    {
        IReadOnlyList<IViewInfo> GetViews(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(IParentViewManager parentViewManager, object view, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}