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

namespace MugenMvvm.Commands
{
    public sealed class CompositeCommand : ComponentOwnerBase<ICompositeCommand>, ICompositeCommand, IHasAddedCallbackComponentOwner, IHasAddingCallbackComponentOwner
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int DisposedState = -1;

        #endregion

        #region Constructors

        public CompositeCommand(IReadOnlyMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public bool HasCanExecute => GetComponents<IConditionCommandComponent>().HasCanExecute();

        public bool IsDisposed => _state == DisposedState;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => GetComponents<IConditionEventCommandComponent>().AddCanExecuteChanged(value);
            remove => GetComponents<IConditionEventCommandComponent>().RemoveCanExecuteChanged(value);
        }

        #endregion

        #region Implementation of interfaces

        public bool CanExecute(object parameter)
        {
            return GetComponents<IConditionCommandComponent>().CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            GetComponents<IExecutorCommandComponent>().ExecuteAsync(parameter);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            base.GetComponents<IDisposable>().Dispose();
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public ActionToken Suspend()
        {
            return GetComponents<ISuspendable>().Suspend();
        }

        public void RaiseCanExecuteChanged()
        {
            GetComponents<IConditionEventCommandComponent>().RaiseCanExecuteChanged();
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
            return IsDisposed ? Default.EmptyArray<TComponent>() : base.GetComponents<TComponent>(metadata);
        }

        public static ICompositeCommand Create(Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand Create(Action execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create(Action execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.GetCommand(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        #endregion
    }
}