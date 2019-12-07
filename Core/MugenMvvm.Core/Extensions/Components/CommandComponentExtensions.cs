using System.Windows.Input;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class CommandComponentExtensions
    {
        #region Methods

        public static ICommandMediator? TryGetCommandMediator(this ICommandMediatorProviderComponent[] components, ICommand command, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetCommandMediator(command, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnCommandMediatorCreated(this ICommandMediatorProviderListener[] listeners, ICommandMediatorProvider provider, ICommandMediator mediator, ICommand command, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCommandMediatorCreated(provider, mediator, command, metadata);
        }

        #endregion
    }
}