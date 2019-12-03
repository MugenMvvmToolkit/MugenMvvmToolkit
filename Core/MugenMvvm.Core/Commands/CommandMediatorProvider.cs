using System.Windows.Input;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
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
            ICommandMediator? result = null;
            var components = GetComponents<ICommandMediatorProviderComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                result = components[i].TryGetCommandMediator(command, metadata);
                if (result != null)
                    break;
            }


            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(ICommandMediatorProviderComponent).Name);//todo move to ext

            var listeners = GetComponents<ICommandMediatorProviderListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                listeners[i].OnCommandMediatorCreated(this, result, command, metadata);

            return result;
        }

        #endregion
    }
}