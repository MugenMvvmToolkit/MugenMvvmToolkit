using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        private const int DisposedState = -1;

        #endregion

        #region Constructors

        public CompositeCommand(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
            CanDispose = true;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public bool IsDisposed => _state == DisposedState;

        public bool CanDispose { get; set; }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => GetComponents<IConditionEventCommandComponent>().AddCanExecuteChanged(this, value, null);
            remove => GetComponents<IConditionEventCommandComponent>().RemoveCanExecuteChanged(this, value, null);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecute(object parameter) => GetComponents<IConditionCommandComponent>().CanExecute(this, parameter, null);

        public void Execute(object parameter) => GetComponents<IExecutorCommandComponent>().ExecuteAsync(this, parameter, null);

        public void Dispose()
        {
            if (!CanDispose || Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            base.GetComponents<IDisposable>().Dispose();
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<ISuspendable>().Suspend(state, metadata);

        public bool HasCanExecute(IReadOnlyMetadataContext? metadata = null) => GetComponents<IConditionCommandComponent>().HasCanExecute(this, metadata);

        public void RaiseCanExecuteChanged(IReadOnlyMetadataContext? metadata = null) => GetComponents<IConditionEventCommandComponent>().RaiseCanExecuteChanged(this, metadata);

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IConditionCommandComponent)
                RaiseCanExecuteChanged();
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        #endregion

        #region Methods

        private new TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? Default.Array<TComponent>() : base.GetComponents<TComponent>(metadata);

        public static ICompositeCommand Create(Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);

        public static ICompositeCommand Create(Action execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create(Action execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null) =>
            MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        #endregion
    }
}