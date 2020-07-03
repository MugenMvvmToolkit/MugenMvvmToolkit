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
    public class CompositeCommand : ComponentOwnerBase<ICompositeCommand>, ICompositeCommand, IHasAddedCallbackComponentOwner, IHasAddingCallbackComponentOwner, IHasDisposeCondition
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;
        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int DisposedState = -1;

        #endregion

        #region Constructors

        public CompositeCommand(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null, IMetadataContextManager? metadataContextManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
            _metadataContextManager = metadataContextManager;
            CanDispose = true;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextManager.LazyInitializeNonReadonly(ref _metadata, this);

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public bool HasCanExecute => GetComponents<IConditionCommandComponent>().HasCanExecute(this);

        public bool IsDisposed => _state == DisposedState;

        public bool CanDispose { get; set; }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => GetComponents<IConditionEventCommandComponent>().AddCanExecuteChanged(this, value);
            remove => GetComponents<IConditionEventCommandComponent>().RemoveCanExecuteChanged(this, value);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecute(object parameter)
        {
            return GetComponents<IConditionCommandComponent>().CanExecute(this, parameter);
        }

        public void Execute(object parameter)
        {
            GetComponents<IExecutorCommandComponent>().ExecuteAsync(this, parameter);
        }

        public void Dispose()
        {
            if (!CanDispose || Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            base.GetComponents<IDisposable>().Dispose();
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public ActionToken Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            return GetComponents<ISuspendable>().Suspend(state, metadata);
        }

        public void RaiseCanExecuteChanged()
        {
            GetComponents<IConditionEventCommandComponent>().RaiseCanExecuteChanged(this);
        }

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IConditionCommandComponent)
                RaiseCanExecuteChanged();
        }

        bool IHasAddingCallbackComponentOwner.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            return !IsDisposed;
        }

        #endregion

        #region Methods

        private new TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            return IsDisposed ? Default.Array<TComponent>() : base.GetComponents<TComponent>(metadata);
        }

        public static ICompositeCommand Create(Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

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
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

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
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

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
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandManager.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

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