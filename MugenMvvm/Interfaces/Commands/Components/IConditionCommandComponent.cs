using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IConditionCommandComponent : IComponent<ICompositeCommand>
    {
        bool HasCanExecute(ICompositeCommand command);

        bool CanExecute(ICompositeCommand command, object? parameter);
    }
}