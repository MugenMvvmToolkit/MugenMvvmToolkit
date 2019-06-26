using System;
using System.Windows.Input;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommand : ICommand, IDisposable, ISuspendable
    {
        ICommandMediator Mediator { get; }

        bool HasCanExecute { get; }

        void RaiseCanExecuteChanged();
    }
}