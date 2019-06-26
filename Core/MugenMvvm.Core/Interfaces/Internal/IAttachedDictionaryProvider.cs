using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedDictionaryProvider : IComponentOwner<IAttachedDictionaryProvider>
    {
        IAttachedDictionary? GetAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null);

        IAttachedDictionary GetOrAddAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null);
    }
}