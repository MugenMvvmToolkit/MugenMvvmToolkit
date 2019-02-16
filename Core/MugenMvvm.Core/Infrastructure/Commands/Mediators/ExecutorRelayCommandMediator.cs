using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Mediators;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    public class ExecutorRelayCommandMediator<T> : IExecutorRelayCommandMediator
    {
        #region Fields

        private bool? _hasCanExecuteImpl;
        private IReadOnlyList<IRelayCommandMediator> _mediators;
        private int _state;

        #endregion

        #region Constructors

        public ExecutorRelayCommandMediator(Delegate executeDelegate, Delegate? canExecuteDelegate, CommandExecutionMode executionMode,
            bool allowMultipleExecution, IReadOnlyList<IRelayCommandMediator> mediators)
        {
            Should.NotBeNull(executeDelegate, nameof(executeDelegate));
            Should.NotBeNull(mediators, nameof(mediators));
            _mediators = mediators;
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

        public IReadOnlyList<IRelayCommandMediator> Mediators => _mediators;

        public virtual bool IsNotificationsSuspended
        {
            get
            {
                var mediators = Mediators;
                for (var i = 0; i < mediators.Count; i++)
                {
                    if (mediators[i] is ISuspendNotifications suspendNotifications && suspendNotifications.IsNotificationsSuspended)
                        return true;
                }

                return false;
            }
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            var emptyArray = Default.EmptyArray<IRelayCommandMediator>();
            if (ReferenceEquals(_mediators, emptyArray))
                return;

            var mediators = Interlocked.Exchange(ref _mediators, Default.EmptyArray<IRelayCommandMediator>());
            if (!ReferenceEquals(mediators, emptyArray))
                OnDispose(mediators);
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
                    var wrapper = (ExecutorRelayCommandMediator<T>)o;
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

            var mediators = Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is IConditionRelayCommandMediator m && m.HasCanExecute())
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

            var mediators = Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is IConditionRelayCommandMediator m && !m.CanExecute(parameter))
                    return false;
            }

            return true;
        }

        public virtual void AddCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var mediators = Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is IConditionEventRelayCommandMediator m)
                    m.AddCanExecuteChanged(handler);
            }
        }

        public virtual void RemoveCanExecuteChanged(EventHandler handler)
        {
            if (!HasCanExecute())
                return;

            var mediators = Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is IConditionEventRelayCommandMediator m)
                    m.RemoveCanExecuteChanged(handler);
            }
        }

        public virtual void RaiseCanExecuteChanged()
        {
            if (!HasCanExecute())
                return;

            var mediators = Mediators;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is IConditionEventRelayCommandMediator m)
                    m.RaiseCanExecuteChanged();
            }
        }

        public virtual IDisposable SuspendNotifications()
        {
            var mediators = Mediators;
            List<IDisposable>? tokens = null;
            for (var i = 0; i < mediators.Count; i++)
            {
                if (mediators[i] is ISuspendNotifications suspendNotifications)
                {
                    if (tokens == null)
                        tokens = new List<IDisposable>(2);
                    tokens.Add(suspendNotifications.SuspendNotifications());
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

        protected virtual void OnDispose(IReadOnlyList<IRelayCommandMediator> mediators)
        {
            for (var i = 0; i < mediators.Count; i++)
                mediators[i].Dispose();
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
            return ((Func<T, bool>)canExecuteDelegate).Invoke((T)parameter);
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

        #endregion
    }
}