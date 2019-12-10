using System;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class DelegateExecutorCommandComponent<T> : IExecutorCommandComponent, IConditionCommandComponent, IDisposable, IHasPriority
    {
        #region Fields

        private Delegate? _canExecute;
        private Delegate? _execute;

        #endregion

        #region Constructors

        public DelegateExecutorCommandComponent(Delegate execute, Delegate? canExecute)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region Properties

        public int Priority => CommandComponentPriority.Executor;

        #endregion

        #region Implementation of interfaces

        public bool HasCanExecute()
        {
            return _canExecute != null;
        }

        public bool CanExecute(object? parameter)
        {
            var canExecuteDelegate = _canExecute;
            if (canExecuteDelegate == null)
                return false;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>) canExecuteDelegate).Invoke((T) parameter!);
        }

        public void Dispose()
        {
            _canExecute = null;
            _execute = null;
        }

        public Task ExecuteAsync(object? parameter)
        {
            var executeAction = _execute;
            if (executeAction == null)
                return Default.CompletedTask;

            if (executeAction is Action execute)
            {
                execute();
                return Default.CompletedTask;
            }

            if (executeAction is Action<T> genericExecute)
            {
                genericExecute((T) parameter!);
                return Default.CompletedTask;
            }

            if (executeAction is Func<Task> executeTask)
                return executeTask();
            return ((Func<T, Task>) executeAction).Invoke((T) parameter!);
        }

        #endregion
    }
}