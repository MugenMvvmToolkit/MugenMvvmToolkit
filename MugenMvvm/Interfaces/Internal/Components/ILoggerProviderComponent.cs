using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILoggerProviderComponent : IComponent<ILogger>
    {
        ILogger? TryGetLogger(ILogger logger, object request, IReadOnlyMetadataContext? metadata);
    }
}