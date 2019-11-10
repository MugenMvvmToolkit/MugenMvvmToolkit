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

        public ICommandMediator GetCommandMediator<TParameter>(ICommand command, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(metadata, nameof(metadata));
            ICommandMediator? result = null;
            var components = Components.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                result = (components[i] as ICommandMediatorProviderComponent)?.TryGetCommandMediator<TParameter>(command, metadata);
                if (result != null)
                    break;
            }


            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this, typeof(ICommandMediatorProviderComponent).Name);

            for (var i = 0; i < components.Length; i++)
                (components[i] as ICommandMediatorProviderListener)?.OnCommandMediatorCreated<TParameter>(this, result, command, metadata);

            return result!;
        }

        #endregion
    }
}