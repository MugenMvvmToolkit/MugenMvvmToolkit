using System;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IConditionEventRelayCommandMediator : IRelayCommandMediator
    {
        void AddCanExecuteChanged(EventHandler handler);

        void RemoveCanExecuteChanged(EventHandler handler);

        void RaiseCanExecuteChanged();
    }
}