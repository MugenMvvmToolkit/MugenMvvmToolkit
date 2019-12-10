using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewInitializer : IMetadataOwner<IReadOnlyMetadataContext>, IHasId<string>
    {
        Type ViewType { get; }

        Type ViewModelType { get; }

        Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext? metadata = null);

        Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null);

        Task<IViewInitializerResult> InitializeAsync(object view, IReadOnlyMetadataContext? metadata = null);

        Task<IReadOnlyMetadataContext> CleanupAsync(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null);
    }
}