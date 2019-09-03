using System;
using System.Collections.Generic;
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
                var components = GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] is ISuspendable suspendNotifications && suspendNotifications.IsSuspended)
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

            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionCommandMediatorComponent m && m.HasCanExecute())
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

            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is IConditionCommandMediatorComponent m && !m.CanExecute(parameter))
                    return false;
            }

            return true;
        }

        public virtual void AddCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IConditionEventCommandMediatorComponent)?.AddCanExecuteChanged(handler);
        }

        public virtual void RemoveCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IConditionEventCommandMediatorComponent)?.RemoveCanExecuteChanged(handler);
        }

        public virtual void RaiseCanExecuteChanged()
        {
            if (!HasCanExecute())
                return;

            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IConditionEventCommandMediatorComponent)?.RaiseCanExecuteChanged();
        }

        public virtual IDisposable Suspend()
        {
            var components = GetComponents();
            List<IDisposable>? tokens = null;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ISuspendable suspendNotifications)
                {
                    if (tokens == null)
                        tokens = new List<IDisposable>(2);
                    tokens.Add(suspendNotifications.Suspend());
                }
            }

            if (tokens == null)
                return Default.Disposable;
            return WeakActionToken.Create(tokens, t =>
            {
                for (var i = 0; i < t.Count; i++)
                    t[i].Dispose();
            });
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
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDisposable)?.Dispose();

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
            switch (ExecutionMode)
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
                        ExceptionManager.ThrowCommandCannotBeExecuted();
                    break;
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