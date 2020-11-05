using System;
using System.Collections.Generic;
using MugenMvvm.Enums;

namespace MugenMvvm.Commands
{
    public class DelegateCommandRequest
    {
        #region Constructors

        public DelegateCommandRequest(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
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

        #region Properties

        public bool? AllowMultipleExecution { get; protected set; }

        public Delegate? CanExecute { get; protected set; }

        public Func<object, bool>? CanNotify { get; protected set; }

        public ThreadExecutionMode? EventThreadMode { get; protected set; }

        public Delegate Execute { get; protected set; }

        public CommandExecutionBehavior? ExecutionMode { get; protected set; }

        public IReadOnlyList<object>? Notifiers { get; protected set; }

        #endregion

        #region Methods

        public static object Get(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify)
        {
            if (canExecute == null && allowMultipleExecution == null && executionMode == null && eventThreadMode == null)
                return execute;
            return new DelegateCommandRequest(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
        }

        #endregion
    }
}