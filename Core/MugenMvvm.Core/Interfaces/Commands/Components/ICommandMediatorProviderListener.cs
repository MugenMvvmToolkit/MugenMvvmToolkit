using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandMediatorProviderListener : IComponent<ICommandMediatorProvider>
    {
        void OnCommandMediatorCreated(ICommandMediatorProvider provider, ICommandMediator mediator, ICommand command, IReadOnlyMetadataContext metadata);
    }
}