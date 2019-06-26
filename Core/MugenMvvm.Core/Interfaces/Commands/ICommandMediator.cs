using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands.Components
{
    public interface ICommandMediator : IComponentOwner<ICommandMediator>, ISuspendable, IDisposable
    {
        void AddCanExecuteChanged(EventHandler handler);

        void RemoveCanExecuteChanged(EventHandler handler);

        void RaiseCanExecuteChanged();

        bool HasCanExecute();

        bool CanExecute(object? parameter);

        Task ExecuteAsync(object? parameter);
    }
}