using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProvider
    {
        IComponentCollection<IChildAttachedValueProvider> Providers { get; }

        IAttachedValueProviderDictionary GetAttachedDictionary(object item, IReadOnlyMetadataContext metadata);

        IAttachedValueProviderDictionary? GetAttachedDictionaryOptional(object item, IReadOnlyMetadataContext metadata);
    }
}