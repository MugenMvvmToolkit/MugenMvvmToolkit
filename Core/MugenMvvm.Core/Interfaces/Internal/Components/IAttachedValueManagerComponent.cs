using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueManagerComponent : IComponent<IAttachedValueManager>
    {
        bool TryGetAttachedValueProvider<TItem>(TItem item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? dictionary)
            where TItem : class;

        bool TryGetOrAddAttachedValueProvider<TItem>(TItem item, IReadOnlyMetadataContext? metadata, [NotNullWhen(true)] out IAttachedValueProvider? dictionary)
            where TItem : class;
    }
}