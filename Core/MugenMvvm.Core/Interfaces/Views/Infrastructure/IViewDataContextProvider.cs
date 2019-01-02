using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewDataContextProvider
    {
        object? GetDataContext(object view, IReadOnlyMetadataContext metadata);

        void SetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata);
    }
}