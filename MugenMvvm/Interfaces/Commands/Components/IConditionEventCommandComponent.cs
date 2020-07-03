using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IConditionEventCommandComponent : IComponent<ICompositeCommand>
    {
        void AddCanExecuteChanged(ICompositeCommand command, EventHandler handler);

        void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler handler);

        void RaiseCanExecuteChanged(ICompositeCommand command);
    }
}