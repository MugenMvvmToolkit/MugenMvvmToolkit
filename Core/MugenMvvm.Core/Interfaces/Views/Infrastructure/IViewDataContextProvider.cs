using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewDataContextProvider
    {
        IComponentCollection<IChildViewDataContextProvider> Providers { get; }

        object? GetDataContext(object view, IReadOnlyMetadataContext metadata);

        bool SetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata);
    }
}