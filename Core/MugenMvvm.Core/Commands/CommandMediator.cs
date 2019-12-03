using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands
{
    public class CommandMediator : ComponentOwnerBase<ICommandMediator>, ICommandMediator, IHasAddedCallbackComponentOwner, IHasRemovedCallbackComponentOwner
    {
        #region Fields

        private bool? _hasCanExecuteImpl;
        private int _state;

        #endregion

        #region Constructors

        public CommandMediator(IComponentCollectionProvider? componentCollectionProvider, CommandExecutionMode executionMode, bool allowMultipleExecution)
            : base(componentCollectionProvider)
        {
            ExecutionMode = executionMode;
            AllowMultipleExecution = allowMultipleExecution;
        }

        #endregion

        #region Properties

        protected CommandExecutionMode ExecutionMode { get; }

        protected bool AllowMultipleExecution { get; }

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
                    var wrapper = (CommandMediator) o;
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
            if (!AllowMultipleExecution)
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

            var tokens = new ActionToken[components.Length];
            for (var i = 0; i < components.Length; i++)
                tokens[i] = components[i].Suspend();

            return new ActionToken((o, _) =>
            {
                var list = (ActionToken[]) o!;
                for (var i = 0; i < list.Length; i++)
                    list[i].Dispose();
            }, tokens);
        }

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (_hasCanExecuteImpl != null && component is IConditionCommandMediatorComponent)
                _hasCanExecuteImpl = null;
        }

        void IHasRemovedCallbackComponentOwner.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (_hasCanExecuteImpl != null && component is IConditionCommandMediatorComponent)
                _hasCanExecuteImpl = null;
        }

        #endregion

        #region Methods

        protected virtual void OnDispose()
        {
            var components = GetComponents<IDisposable>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].Dispose();
        }

        protected virtual bool CanExecuteInternal(object? parameter)
        {
            return _state == 0;
        }

        protected virtual Task ExecuteInternalAsync(object? parameter)
        {
            if (ExecutionMode == CommandExecutionMode.CanExecuteBeforeExecute)
            {
                if (!CanExecute(parameter))
                {
                    RaiseCanExecuteChanged();
                    return Default.CompletedTask;
                }
            }
            else if (ExecutionMode == CommandExecutionMode.CanExecuteBeforeExecuteException)
            {
                if (!CanExecute(parameter))
                    ExceptionManager.ThrowCommandCannotBeExecuted();
            }

            var components = Components.GetComponents<IExecutorCommandMediatorComponent>(null);
            if (components.Length == 0)
                ExceptionManager.ThrowObjectNotInitialized(this, components);
            return components[0].ExecuteAsync(parameter);
        }

        #endregion
    }
}