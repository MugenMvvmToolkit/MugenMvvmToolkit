using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedDictionaryProvider : IComponentOwner<IAttachedDictionaryProvider>, IComponent<IMugenApplication>
    {
        IAttachedDictionary? GetAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null);

        IAttachedDictionary GetOrAddAttachedDictionary(object item, IReadOnlyMetadataContext? metadata = null);
    }
}