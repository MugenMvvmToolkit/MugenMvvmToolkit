using System.Windows.Input;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Commands
{
    public interface ICommandMediatorProvider : IComponentOwner<ICommandMediatorProvider>, IComponent<IMugenApplication>
    {
        ICommandMediator GetCommandMediator<TParameter>(ICommand command, IReadOnlyMetadataContext metadata);
    }
}