using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewModelViewInitializer : IViewManagerInitializer
    {
        Type ViewModelType { get; }

        IViewManagerResult<IViewModelBase> Initialize(object view, IReadOnlyMetadataContext metadata);
    }
}