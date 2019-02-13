using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewInitializer : IViewManagerInitializer
    {
        Type ViewType { get; }

        Task<IViewManagerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext metadata);
    }
}