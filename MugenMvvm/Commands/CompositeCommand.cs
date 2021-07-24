using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

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

        public bool IsSuspended => GetComponents<ISuspendableComponent<ICompositeCommand>>().IsSuspended(this, null);

        public static ICompositeCommand Create(object? owner, IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<object?>(owner, CommandMetadata.RawCommandRequest, metadata);

        public static ICompositeCommand Create(object? owner, object request, IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.DefaultIfNull(owner).GetCommand<object?>(owner, request, metadata);

        public static ICompositeCommand Create(object? owner, Action<IReadOnlyMetadataContext?> execute, Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null,
            ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand Create<T>(object? owner, Action<T, IReadOnlyMetadataContext?> execute, Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask(object? owner, Func<CancellationToken, IReadOnlyMetadataContext?, Task> execute,
            Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null,
            ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask(object? owner, Func<CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute,
            Func<IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default,
            bool? allowMultipleExecution = null, ThreadExecutionMode? eventThreadMode = null, Func<object?, object?, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask<T>(object? owner, Func<T, CancellationToken, IReadOnlyMetadataContext?, Task> execute,
            Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null,
            ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

        public static ICompositeCommand CreateFromTask<T>(object? owner, Func<T, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> execute,
            Func<T, IReadOnlyMetadataContext?, bool>? canExecute = null,
            ItemOrIEnumerable<object> notifiers = default, bool? allowMultipleExecution = null, ThreadExecutionMode? eventThreadMode = null,
            Func<object?, object?, bool>? canNotify = null, IReadOnlyMetadataContext? metadata = null, ICommandManager? commandManager = null) =>
            commandManager.GetCommand(owner, execute, canExecute, notifiers, allowMultipleExecution, eventThreadMode, canNotify, metadata);

#pragma warning disable 4014
        public void Execute(object? parameter = null) => ExecuteAsync(parameter).LogException(UnhandledExceptionType.Command);
#pragma warning restore 4014

        public ValueTask<bool> ExecuteAsync(object? parameter = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ICommandExecutorComponent>().TryExecuteAsync(this, parameter, cancellationToken, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter, IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandConditionComponent>().CanExecute(this, parameter, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => GetComponents<ICommandEventHandlerComponent>().RaiseCanExecuteChanged(this, metadata);

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposedState, DefaultState) == DefaultState)
            {
                base.GetComponents<IDisposableComponent<ICompositeCommand>>().Dispose(this, null);
                this.RemoveComponents<ICommandEventHandlerComponent>();
                this.ClearComponents();
                this.ClearMetadata(true);
            }
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ISuspendableComponent<ICompositeCommand>>().TrySuspend(this, state, metadata);

        private new ItemOrArray<TComponent> GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? default : base.GetComponents<TComponent>(metadata);

        bool ICommand.CanExecute(object? parameter) => CanExecute(parameter);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICommandConditionComponent)
                RaiseCanExecuteChanged();
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;
    }
}