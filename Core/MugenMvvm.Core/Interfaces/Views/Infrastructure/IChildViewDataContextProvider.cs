using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IChildViewDataContextProvider : IHasPriority
    {
        bool TryGetDataContext(object view, IReadOnlyMetadataContext metadata, out object? dataContext);

        bool TrySetDataContext(object view, object? dataContext, IReadOnlyMetadataContext metadata);
    }
}