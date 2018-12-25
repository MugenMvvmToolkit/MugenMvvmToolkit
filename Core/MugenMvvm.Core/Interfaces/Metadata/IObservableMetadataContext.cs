using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IObservableMetadataContext : IMetadataContext, IHasListeners<IObservableMetadataContextListener>
    {
    }
}