using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IRelayCommandMediator : ISuspendNotifications, IDisposable
    {
        bool HasCanExecute { get; }

        TMediator GetMediator<TMediator>() where TMediator : class?;

        void AddCanExecuteChanged(EventHandler handler);

        void RemoveCanExecuteChanged(EventHandler handler);

        bool CanExecute(object parameter);

        Task ExecuteAsync(object parameter);

        void RaiseCanExecuteChanged();
    }
}