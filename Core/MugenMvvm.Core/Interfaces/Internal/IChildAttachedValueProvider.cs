using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildAttachedValueProvider : IHasPriority
    {
        bool TryGetOrAddAttachedDictionary<TItem>(IAttachedValueProvider parentProvider, TItem item, bool optional, IReadOnlyMetadataContext metadata, out IAttachedValueProviderDictionary? dictionary)
            where TItem : class;
    }
}