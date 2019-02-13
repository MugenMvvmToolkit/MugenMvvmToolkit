using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewModelViewInitializer : IViewManagerInitializer
    {
        Type ViewModelType { get; }

        Task<IViewManagerResult> InitializeAsync(object view, IReadOnlyMetadataContext metadata);
    }
}