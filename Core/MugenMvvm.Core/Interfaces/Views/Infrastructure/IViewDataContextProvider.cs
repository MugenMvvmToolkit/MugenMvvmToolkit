using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewDataContextProvider//todo merge with bindings
    {
        IComponentCollection<IChildViewDataContextProvider> Providers { get; }

        object? GetDataContext(object view, IReadOnlyMetadataContext metadata);

        bool SetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata);
    }
}