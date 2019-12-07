using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Commands
{
    public sealed class CommandMediatorProvider : ComponentOwnerBase<ICommandMediatorProvider>, ICommandMediatorProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public CommandMediatorProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public ICommandMediator GetCommandMediator(ICommand command, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(metadata, nameof(metadata));
            var result = GetComponents<ICommandMediatorProviderComponent>(metadata).TryGetCommandMediator(command, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            GetComponents<ICommandMediatorProviderListener>(metadata).OnCommandMediatorCreated(this, result, command, metadata);
            return result;
        }

        #endregion
    }
}