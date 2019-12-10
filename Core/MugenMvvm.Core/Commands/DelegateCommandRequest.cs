using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Commands
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct DelegateCommandRequest
    {
        #region Fields

        public readonly bool? AllowMultipleExecution;
        public readonly Delegate? CanExecute;
        public readonly Func<object, bool>? CanNotify;
        public readonly ThreadExecutionMode? EventThreadMode;
        public readonly Delegate Execute;
        public readonly CommandExecutionMode? ExecutionMode;
        public readonly FuncIn<DelegateCommandRequest, IProvider, IReadOnlyMetadataContext?, ICompositeCommand?> Factory;
        public readonly IReadOnlyList<object>? Notifiers;

        #endregion

        #region Constructors

        public DelegateCommandRequest(FuncIn<DelegateCommandRequest, IProvider, IReadOnlyMetadataContext?, ICompositeCommand?> factory,
            Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify)
        {
            Should.NotBeNull(factory, nameof(factory));
            Should.NotBeNull(execute, nameof(execute));
            Factory = factory;
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

        public bool IsEmpty => Execute == null;

        #endregion

        #region Nested types

        public interface IProvider
        {
            ICompositeCommand? TryGetCommand<T>(in DelegateCommandRequest request, IReadOnlyMetadataContext? metadata);
        }

        #endregion
    }
}