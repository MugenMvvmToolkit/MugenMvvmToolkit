using System;
using System.Windows.Input;
using MugenMvvm.Interfaces.Commands.Mediators;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommand : ICommand, IDisposable, ISuspendNotifications
    {
        IExecutorRelayCommandMediator Mediator { get; }

        bool HasCanExecute { get; }

        void RaiseCanExecuteChanged();
    }
}