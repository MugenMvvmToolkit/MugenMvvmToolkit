using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands
{
    public class DelegateCommandRequest
    {
        #region Fields

        private object? _notifiers;

        #endregion

        #region Constructors

        public DelegateCommandRequest(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
            ThreadExecutionMode? eventThreadMode, ItemOrList<object, IReadOnlyList<object>> notifiers, Func<object?, object?, bool>? canNotify)
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

        #endregion

        #region Properties

        public bool? AllowMultipleExecution { get; protected set; }

        public Delegate? CanExecute { get; protected set; }

        public Func<object?, object?, bool>? CanNotify { get; protected set; }

        public ThreadExecutionMode? EventThreadMode { get; protected set; }

        public Delegate Execute { get; protected set; }

        public CommandExecutionBehavior? ExecutionMode { get; protected set; }

        public ItemOrList<object, IReadOnlyList<object>> Notifiers
        {
            get => ItemOrList.FromItem<object>(_notifiers);
            protected set => _notifiers = value.GetRawValue();
        }

        #endregion

        #region Methods

        public static object Get(Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionBehavior? executionMode,
            ThreadExecutionMode? eventThreadMode, ItemOrList<object, IReadOnlyList<object>> notifiers, Func<object?, object?, bool>? canNotify)
        {
            if (canExecute == null && allowMultipleExecution == null && executionMode == null && eventThreadMode == null)
                return execute;
            return new DelegateCommandRequest(execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
        }

        #endregion
    }
}