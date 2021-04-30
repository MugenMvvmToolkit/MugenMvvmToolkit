using System;
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
        private const int DefaultState = 0;
        private const int NoDisposeState = 1;
        private const int DisposedState = 2;

        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        public CompositeCommand(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => GetComponents<ICommandEventHandlerComponent>().AddCanExecuteChanged(this, value, null);
            remove => GetComponents<ICommandEventHandlerComponent>().RemoveCanExecuteChanged(this, value, null);
        }

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

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public static ICompositeCommand Create(object? owner, Action execute, Func<bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenExtensions.GetCommand(null, owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand Create<T>(object? owner, Action<T> execute, Func<T, bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenExtensions.GetCommand(null, owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask(object? owner, Func<Task> execute, Func<bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null,
            CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenExtensions.GetCommand(null, owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask<T>(object? owner, Func<T, Task> execute, Func<T, bool>? canExecute = null, ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, CommandExecutionBehavior? executionMode = null, ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenExtensions.GetCommand(null, owner, execute, canExecute, notifiers, allowMultipleExecution, executionMode, eventThreadMode, canNotify, metadata);

        public bool CanExecute(object? parameter) => GetComponents<ICommandConditionComponent>().CanExecute(this, parameter, null);

        public void Execute(object? parameter) => ExecuteAsync(parameter).LogException(UnhandledExceptionType.Command);

        public ValueTask<bool> ExecuteAsync(object? parameter, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ICommandExecutorComponent>().ExecuteAsync(this, parameter, cancellationToken, metadata);

        public bool HasCanExecute(IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandConditionComponent>().HasCanExecute(this, metadata);

        public void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandEventHandlerComponent>().RaiseCanExecuteChanged(this, metadata);

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

        private new ItemOrArray<TComponent> GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? default : base.GetComponents<TComponent>(metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICommandConditionComponent)
                RaiseCanExecuteChanged();
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;
    }
}