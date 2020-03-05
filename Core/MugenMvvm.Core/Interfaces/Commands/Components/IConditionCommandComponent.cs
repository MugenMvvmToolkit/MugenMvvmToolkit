using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IConditionCommandComponent : IComponent<ICompositeCommand>
    {
        bool HasCanExecute();

        bool CanExecute(object? parameter);
    }
}