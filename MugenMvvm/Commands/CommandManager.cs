using System.Diagnostics.CodeAnalysis;
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
        #region Constructors

        [Preserve(Conditional = true)]
        public CommandManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ICompositeCommand? TryGetCommand<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<ICommandProviderComponent>(metadata).TryGetCommand(request, metadata);
            if (result != null)
                GetComponents<ICommandManagerListener>(metadata).OnCommandCreated(this, result, request, metadata);
            return result;
        }

        #endregion
    }
}