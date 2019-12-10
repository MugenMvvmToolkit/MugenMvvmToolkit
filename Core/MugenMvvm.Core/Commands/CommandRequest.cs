using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands.Components;

namespace MugenMvvm.Commands
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CommandRequest
    {
        #region Fields

        public readonly bool? AllowMultipleExecution;
        public readonly ThreadExecutionMode? EventThreadMode;
        public readonly CommandExecutionMode? ExecutionMode;
        public readonly IExecutorCommandComponent Executor;
        public readonly Func<object, bool>? CanNotify;
        public readonly IReadOnlyCollection<object>? Notifiers;

        #endregion

        #region Constructors

        public CommandRequest(IExecutorCommandComponent executor, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyCollection<object>? notifiers, Func<object, bool>? canNotify)
        {
            Should.NotBeNull(executor, nameof(executor));
            Executor = executor;
            AllowMultipleExecution = allowMultipleExecution;
            ExecutionMode = executionMode;
            EventThreadMode = eventThreadMode;
            Notifiers = notifiers;
            CanNotify = canNotify;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Executor == null;

        #endregion
    }
}