using System;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;

namespace MugenMvvm.Commands
{
    public class DelegateCommandRequest
    {
        private object? _notifiers;

        public DelegateCommandRequest(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
            ThreadExecutionMode? eventThreadMode, ItemOrIEnumerable<object> notifiers, Func<object?, object?, bool>? canNotify)
        {
            Should.NotBeNull(execute, nameof(execute));
            Execute = execute;
            CanExecute = canExecute;
            AllowMultipleExecution = allowMultipleExecution;
            ExecutionMode = executionMode;
            EventThreadMode = eventThreadMode;
            _notifiers = notifiers.GetRawValue();
            CanNotify = canNotify;
        }

        public bool? AllowMultipleExecution { get; protected set; }

        public Delegate? CanExecute { get; protected set; }

        public Func<object?, object?, bool>? CanNotify { get; protected set; }

        public ThreadExecutionMode? EventThreadMode { get; protected set; }

        public Delegate Execute { get; protected set; }

        public CommandExecutionBehavior? ExecutionMode { get; protected set; }

        public ItemOrIEnumerable<object> Notifiers
        {
            get => ItemOrIEnumerable.FromRawValue<object>(_notifiers);
            protected set => _notifiers = value.GetRawValue();
        }

        public static object Get(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
            ThreadExecutionMode? eventThreadMode, ItemOrIEnumerable<object> notifiers, Func<object?, object?, bool>? canNotify)
        {
            if (canExecute == null && allowMultipleExecution == null && executionMode == null && eventThreadMode == null)
                return execute;
            return new DelegateCommandRequest(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
        }
    }
}