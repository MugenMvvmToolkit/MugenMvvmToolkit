using System;
using System.Windows.Input;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommand : ICommand, IDisposable
    {
        IRelayCommandMediator Mediator { get; }
    }
}