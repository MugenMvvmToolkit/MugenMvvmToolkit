using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface IConditionEventCommandComponent : IComponent<ICompositeCommand>
    {
        void AddCanExecuteChanged(EventHandler handler);

        void RemoveCanExecuteChanged(EventHandler handler);

        void RaiseCanExecuteChanged();
    }
}