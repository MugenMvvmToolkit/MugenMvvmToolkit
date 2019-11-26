using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueManagerComponent : IComponent<IAttachedValueManager>
    {
        bool TryGetAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider);

        bool TryGetOrAddAttachedValueProvider(object item, IReadOnlyMetadataContext? metadata, [NotNullWhen(true)] out IAttachedValueProvider? provider);
    }
}