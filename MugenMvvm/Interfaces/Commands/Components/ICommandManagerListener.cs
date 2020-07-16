using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandManagerListener : IComponent<ICommandManager>
    {
        void OnCommandCreated<TParameter>(ICommandManager provider, ICompositeCommand command, object request, IReadOnlyMetadataContext? metadata);
    }
}