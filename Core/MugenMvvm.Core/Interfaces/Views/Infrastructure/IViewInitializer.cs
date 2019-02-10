using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewInitializer : IViewManagerInitializer
    {
        Type ViewType { get; }

        IViewManagerResult<IViewInfo> Initialize(IViewModel viewModel, IReadOnlyMetadataContext metadata);
    }
}