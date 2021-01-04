using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandManagerListener : IComponent<ICommandManager>
    {
        void OnCommandCreated<TParameter>(ICommandManager commandManager, ICompositeCommand command, object? owner, object request, IReadOnlyMetadataContext? metadata);
    }
}