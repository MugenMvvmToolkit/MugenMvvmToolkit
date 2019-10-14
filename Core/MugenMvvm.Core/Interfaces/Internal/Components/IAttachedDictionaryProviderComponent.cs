using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedDictionaryProviderComponent : IComponent<IAttachedDictionaryProvider>
    {
        bool TryGetAttachedDictionary<TItem>(TItem item, IReadOnlyMetadataContext? metadata, out IAttachedDictionary? dictionary)
            where TItem : class;

        bool TryGetOrAddAttachedDictionary<TItem>(TItem item, IReadOnlyMetadataContext? metadata, [NotNullWhen(true)] out IAttachedDictionary? dictionary)
            where TItem : class;
    }
}