using System;

namespace MugenMvvm.Interfaces.Commands.Mediators
{
    public interface IConditionEventRelayCommandMediator : IRelayCommandMediator
    {
        void AddCanExecuteChanged(EventHandler handler);

        void RemoveCanExecuteChanged(EventHandler handler);

        void RaiseCanExecuteChanged();
    }
}