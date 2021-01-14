using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Commands
{
    public sealed class CommandManager : ComponentOwnerBase<ICommandManager>, ICommandManager
    {
        [Preserve(Conditional = true)]
        public CommandManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        public ICompositeCommand? TryGetCommand<TParameter>(object? owner, object request, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<ICommandProviderComponent>(metadata).TryGetCommand<TParameter>(this, owner, request, metadata);
            if (result != null)
                GetComponents<ICommandManagerListener>(metadata).OnCommandCreated<TParameter>(this, result, owner, request, metadata);
            return result;
        }
    }
}