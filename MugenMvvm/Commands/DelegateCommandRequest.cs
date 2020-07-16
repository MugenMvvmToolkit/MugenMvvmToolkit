using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Commands
{
    public class DelegateCommandRequest
    {
        #region Fields

        public readonly bool? AllowMultipleExecution;
        public readonly Delegate? CanExecute;
        public readonly Func<object, bool>? CanNotify;
        public readonly ThreadExecutionMode? EventThreadMode;
        public readonly Delegate Execute;
        public readonly CommandExecutionMode? ExecutionMode;
        public readonly IReadOnlyList<object>? Notifiers;

        #endregion

        #region Constructors

        public DelegateCommandRequest(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify)
        {
            Should.NotBeNull(execute, nameof(execute));
            Execute = execute;
            CanExecute = canExecute;
            AllowMultipleExecution = allowMultipleExecution;
            ExecutionMode = executionMode;
            EventThreadMode = eventThreadMode;
            Notifiers = notifiers;
            CanNotify = canNotify;
        }

        #endregion

        #region Methods

        public static object Get(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify)
        {
            if (canExecute == null && allowMultipleExecution == null && executionMode == null && eventThreadMode == null)
                return execute;
            return new DelegateCommandRequest(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
        }

        #endregion
    }
}