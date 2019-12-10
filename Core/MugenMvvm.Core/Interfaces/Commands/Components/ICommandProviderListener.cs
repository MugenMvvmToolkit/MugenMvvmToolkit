using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandProviderListener : IComponent<ICommandProvider>
    {
        void OnCommandCreated<TRequest>(ICommandProvider provider, in TRequest request, ICompositeCommand command, IReadOnlyMetadataContext? metadata);
    }
}