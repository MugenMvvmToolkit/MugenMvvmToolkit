using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderComponent : IComponent<ICommandProvider>
    {
        ICompositeCommand? TryGetCommand<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}