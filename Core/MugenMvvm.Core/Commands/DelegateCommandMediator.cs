using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands
{
    public class DelegateCommandMediator<T> : ComponentOwnerBase<ICommandMediator>, ICommandMediator
    {
        #region Fields

        private bool? _hasCanExecuteImpl;
        private int _state;

        #endregion

        #region Constructors

        public DelegateCommandMediator(IComponentCollectionProvider? componentCollectionProvider, Delegate executeDelegate,
            Delegate? canExecuteDelegate, CommandExecutionMode executionMode, bool allowMultipleExecution)
            : base(componentCollectionProvider)
        {
            Should.NotBeNull(executeDelegate, nameof(executeDelegate));
            ExecuteDelegate = executeDelegate;
            CanExecuteDelegate = canExecuteDelegate;
            ExecutionMode = executionMode;
            AllowMultipleExecution = allowMultipleExecution;
        }

        #endregion

        #region Properties

        protected CommandExecutionMode ExecutionMode { get; }

        protected bool AllowMultipleExecution { get; }

        protected Delegate ExecuteDelegate { get; private set; }

        protected Delegate? CanExecuteDelegate { get; private set; }

        public virtual bool IsSuspended
        {
            get
            {
                var components = GetComponents<ISuspendable>(null);
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i].IsSuspended)
                        return true;
                }

                return false;
            }
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, int.MinValue) != int.MinValue)
                OnDispose();
        }

        public virtual Task ExecuteAsync(object? parameter)
        {
            if (AllowMultipleExecution)
                return ExecuteInternalAsync(parameter);

            if (Interlocked.CompareExchange(ref _state, int.MaxValue, 0) != 0)
                return Default.CompletedTask;

            try
            {
                var executionTask = ExecuteInternalAsync(parameter);
                if (executionTask.IsCompleted)
                {
                    Interlocked.Exchange(ref _state, 0);
                    return Default.CompletedTask;
                }

                RaiseCanExecuteChanged();
                return executionTask.ContinueWith((t, o) =>
                {
                    var wrapper = (DelegateCommandMediator<T>)o;
                    Interlocked.Exchange(ref wrapper._state, 0);
                    wrapper.RaiseCanExecuteChanged();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
            }
            catch
            {
                _state = 0;
                throw;
            }
        }

        public virtual bool HasCanExecute()
        {
            if (!AllowMultipleExecution || CanExecuteDelegate != null)
                return true;

            if (_hasCanExecuteImpl.HasValue)
                return _hasCanExecuteImpl.Value;

            var components = GetComponents<IConditionCommandMediatorComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasCanExecute())
                {
                    _hasCanExecuteImpl = true;
                    return true;
                }
            }

            _hasCanExecuteImpl = false;
            return false;
        }

        public virtual bool CanExecute(object? parameter)
        {
            if (!HasCanExecute())
                return true;

            if (!CanExecuteInternal(parameter))
                return false;

            var components = GetComponents<IConditionCommandMediatorComponent>(null);
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanExecute(parameter))
                    return false;
            }

            return true;
        }

        public virtual void AddCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents<IConditionEventCommandMediatorComponent>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].AddCanExecuteChanged(handler);
        }

        public virtual void RemoveCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents<IConditionEventCommandMediatorComponent>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].RemoveCanExecuteChanged(handler);
        }

        public virtual void RaiseCanExecuteChanged()
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents<IConditionEventCommandMediatorComponent>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].RaiseCanExecuteChanged();
        }

        public virtual ActionToken Suspend()
        {
            var components = GetComponents<ISuspendable>(null);
            if (components.Length == 0)
                return default;
            if (components.Length == 1)
                return components[0].Suspend();

            ActionToken[] tokens = new ActionToken[components.Length];
            for (var i = 0; i < components.Length; i++)
                tokens[i] = components[i].Suspend();

            return new ActionToken((o, _) =>
            {
                var list = (ActionToken[])o!;
                for (int i = 0; i < list.Length; i++)
                    list[i].Dispose();
            }, tokens);
        }

        #endregion

        #region Methods

        public static bool IsSupported(Delegate? executor)
        {
            if (executor == null)
                return false;
            return executor is Action || executor is Action<T> || executor is Func<Task> || executor is Func<T, Task>;
        }

        public static bool IsCanExecuteSupported(Delegate? canExecute)
        {
            if (canExecute == null)
                return true;
            return canExecute is Func<bool> || canExecute is Func<T, bool>;
        }

        protected virtual void OnDispose()
        {
            var components = GetComponents<IDisposable>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].Dispose();

            ExecuteDelegate = Default.NoDoAction;
            CanExecuteDelegate = null;
        }

        protected virtual bool CanExecuteInternal(object? parameter)
        {
            if (_state != 0)
                return false;

            var canExecuteDelegate = CanExecuteDelegate;
            if (canExecuteDelegate == null)
                return false;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>)canExecuteDelegate).Invoke((T)parameter!);
        }

        protected virtual Task ExecuteInternalAsync(object? parameter)
        {
            var executeAction = ExecuteDelegate;
            if (ExecutionMode == CommandExecutionMode.CanExecuteBeforeExecute)
            {
                if (!CanExecute(parameter))
                {
                    Tracer.Warn(MessageConstant.CommandCannotBeExecutedString);
                    RaiseCanExecuteChanged();
                    return Default.CompletedTask;
                }
            }
            else if (ExecutionMode == CommandExecutionMode.CanExecuteBeforeExecuteException)
            {
                if (!CanExecute(parameter))
                    ExceptionManager.ThrowCommandCannotBeExecuted();
            }

            if (executeAction is Action execute)
            {
                execute();
                return Default.CompletedTask;
            }

            if (executeAction is Action<T> genericExecute)
            {
                genericExecute((T)parameter!);
                return Default.CompletedTask;
            }

            if (executeAction is Func<Task> executeTask)
                return executeTask();
            return ((Func<T, Task>)executeAction).Invoke((T)parameter!);
        }

        #endregion
    }
}