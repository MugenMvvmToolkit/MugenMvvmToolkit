using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IChildViewManager : IHasPriority
    {
        IReadOnlyList<IViewInfo> GetViews(IParentViewManager viewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(IParentViewManager viewManager, object view, IReadOnlyMetadataContext metadata);

        IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IParentViewManager viewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}