using System;
using System.Collections.Generic;
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
    public sealed class CompositeCommand : ComponentOwnerBase<ICompositeCommand>, ICompositeCommand
    {
        #region Constructors

        public CompositeCommand(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Properties

        public bool IsSuspended => GetComponents<ISuspendable>().IsSuspended();

        public bool HasCanExecute => GetComponents<IConditionCommandComponent>().HasCanExecute();

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
            GetComponents<IDisposable>().Dispose();
        }

        public ActionToken Suspend()
        {
            return GetComponents<ISuspendable>().Suspend();
        }

        public void RaiseCanExecuteChanged()
        {
            GetComponents<IConditionEventCommandComponent>().RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        public static ICompositeCommand Create(Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyCollection<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.Create(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand Create(Action execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.Create(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create(Action execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.Create(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyCollection<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.Create(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.Create(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand Create<T>(Action<T> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.Create(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyCollection<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask(Func<Task> execute, bool allowMultipleExecution, Func<bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyCollection<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, null, null, null, notifiers);
        }

        public static ICompositeCommand CreateFromTask<T>(Func<T, Task> execute, bool allowMultipleExecution, Func<T, bool> canExecute, params object[] notifiers)
        {
            Should.NotBeNull(canExecute, nameof(canExecute));
            return MugenService.CommandProvider.CreateFromTask(execute, canExecute, allowMultipleExecution, null, null, notifiers);
        }

        #endregion
    }
}