using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    internal sealed class ExecutorRelayCommandMediator<T> : IRelayCommandMediator
    {
        #region Fields

        private readonly CommandExecutionMode _executionMode;
        private Delegate? _executeAction;

        #endregion

        #region Constructors

        public ExecutorRelayCommandMediator(Delegate executeAction, CommandExecutionMode executionMode)
        {
            _executeAction = executeAction;
            _executionMode = executionMode;
        }

        #endregion

        #region Properties

        public bool IsNotificationsSuspended => false;

        public bool HasCanExecute => false;

        #endregion

        #region Implementation of interfaces

        public IDisposable SuspendNotifications()
        {
            return Default.Disposable;
        }

        public void Dispose()
        {
            _executeAction = null;
        }

        public TMediator GetMediator<TMediator>() where TMediator : class?
        {
            return this as TMediator;
        }

        public void AddCanExecuteChanged(EventHandler handler)
        {
        }

        public void RemoveCanExecuteChanged(EventHandler handler)
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public Task ExecuteAsync(object parameter)
        {
            var executeAction = _executeAction;
            if (executeAction == null)
                return Default.CompletedTask;

            switch (_executionMode)
            {
                case CommandExecutionMode.CanExecuteBeforeExecute:
                    if (!CanExecute(parameter))
                    {
                        Tracer.Warn(MessageConstants.CommandCannotBeExecutedString);
                        RaiseCanExecuteChanged();
                        return Default.CompletedTask;
                    }

                    break;
                case CommandExecutionMode.CanExecuteBeforeExecuteWithException:
                    if (!CanExecute(parameter))
                        throw ExceptionManager.CommandCannotBeExecuted();
                    break;
            }

            if (executeAction is Action execute)
            {
                execute();
                return Default.CompletedTask;
            }

            if (executeAction is Action<T> genericExecute)
            {
                genericExecute((T)parameter);
                return Default.CompletedTask;
            }

            if (executeAction is Func<Task> executeTask)
                return executeTask();
            return ((Func<T, Task>)executeAction).Invoke((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
        }

        #endregion
    }
}