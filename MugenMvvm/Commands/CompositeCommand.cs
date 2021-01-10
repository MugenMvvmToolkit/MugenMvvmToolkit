using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands
{
    public class CompositeCommand : ComponentOwnerBase<ICompositeCommand>, ICompositeCommand, IHasComponentAddedHandler, IHasComponentAddingHandler, IHasDisposeCondition
    {
        #region Fields

        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int DefaultState = 0;
        private const int NoDisposeState = 1;
        private const int DisposedState = 2;

        #endregion

        #region Constructors

        public CompositeCommand(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public bool IsDisposed => _state == DisposedState;

        public bool IsDisposable
        {
            get => _state == DefaultState;
            set
            {
                if (value)
                    Interlocked.CompareExchange(ref _state, DefaultState, NoDisposeState);
                else
                    Interlocked.CompareExchange(ref _state, NoDisposeState, DefaultState);
            }
        }

        #endregion

        #region Events

        public event EventHandler? CanExecuteChanged
        {
            add => GetComponents<ICommandEventHandlerComponent>().AddCanExecuteChanged(this, value, null);
            remove => GetComponents<ICommandEventHandlerComponent>().RemoveCanExecuteChanged(this, value, null);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecute(object? parameter) => GetComponents<ICommandConditionComponent>().CanExecute(this, parameter, null);

        public void Execute(object? parameter) => ExecuteAsync(parameter);

        public Task ExecuteAsync(object? parameter, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ICommandExecutorComponent>().ExecuteAsync(this, parameter, cancellationToken, metadata);

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposedState, DefaultState) == DefaultState)
            {
                base.GetComponents<IDisposable>().Dispose();
                this.ClearComponents();
                this.ClearMetadata(true);
            }
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<ISuspendable>().Suspend(state, metadata);

        public bool HasCanExecute(IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandConditionComponent>().HasCanExecute(this, metadata);

        public void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandEventHandlerComponent>().RaiseCanExecuteChanged(this, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICommandConditionComponent)
                RaiseCanExecuteChanged();
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        #endregion

        #region Methods

        private new TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? Default.Array<TComponent>() : base.GetComponents<TComponent>(metadata);

        public static ICompositeCommand Create(object? owner, Action execute, Func<bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand Create<T>(object? owner, Action<T> execute, Func<T, bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask(object? owner, Func<Task> execute, Func<bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask<T>(object? owner, Func<T, Task> execute, Func<T, bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        #endregion
    }
}