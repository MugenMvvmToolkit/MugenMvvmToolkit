using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewDataContextProvider//todo listeners
    {
        object? GetDataContext(object view, IReadOnlyMetadataContext metadata);

        void SetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata);
    }
}