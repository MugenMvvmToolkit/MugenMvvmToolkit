using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IConditionCommandMediatorComponent : IComponent<ICommandMediator>
    {
        bool HasCanExecute();

        bool CanExecute(object? parameter);
    }
}