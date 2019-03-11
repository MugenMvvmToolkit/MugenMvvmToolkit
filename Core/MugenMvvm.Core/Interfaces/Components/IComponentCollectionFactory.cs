using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionFactory
    {
        IComponentCollection<T> GetComponentCollection<T>(object target, IReadOnlyMetadataContext metadata) where T : class;//todo atomic create!!!        
    }
}