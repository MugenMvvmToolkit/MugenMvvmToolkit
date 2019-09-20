using System;
using System.Windows.Input;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IMediatorCommand : ICommand, IDisposable, ISuspendable
    {
        ICommandMediator Mediator { get; }

        bool HasCanExecute { get; }

        void RaiseCanExecuteChanged();
    }
}