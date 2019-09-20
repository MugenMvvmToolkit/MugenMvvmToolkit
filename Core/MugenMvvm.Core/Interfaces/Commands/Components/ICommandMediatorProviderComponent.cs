using System.Windows.Input;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandMediatorProviderComponent : IComponent<ICommandMediatorProvider>
    {
        ICommandMediator? TryGetCommandMediator<TParameter>(ICommand command, IReadOnlyMetadataContext metadata);
    }
}