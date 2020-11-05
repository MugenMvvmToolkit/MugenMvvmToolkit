using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public class CommandExecutionBehavior : EnumBase<CommandExecutionBehavior, int>
    {
        #region Fields

        private readonly Action<ICommand, object?>? _afterExecute;
        private readonly Func<ICommand, object?, bool>? _beforeExecute;

        public static readonly CommandExecutionBehavior None = new CommandExecutionBehavior(0, null, null);
        public static readonly CommandExecutionBehavior CanExecuteBeforeExecute = new CommandExecutionBehavior(1, CheckCanExecute, null);
        public static readonly CommandExecutionBehavior CanExecuteBeforeExecuteException = new CommandExecutionBehavior(2, CheckCanExecuteThrow, null);

        #endregion

        #region Constructors

        public CommandExecutionBehavior(int value, Func<ICommand, object?, bool>? beforeExecute, Action<ICommand, object?>? afterExecute) : base(value)
        {
            _beforeExecute = beforeExecute;
            _afterExecute = afterExecute;
        }

        #endregion

        #region Methods

        public bool BeforeExecute(ICommand command, object? parameter) => _beforeExecute?.Invoke(command, parameter) ?? true;

        public void AfterExecute(ICommand command, object? parameter) => _afterExecute?.Invoke(command, parameter);

        private static bool CheckCanExecute(ICommand command, object? parameter)
        {
            if (command.CanExecute(parameter))
                return true;
            (command as ICompositeCommand)?.RaiseCanExecuteChanged();
            return false;
        }

        private static bool CheckCanExecuteThrow(ICommand command, object? parameter)
        {
            if (!command.CanExecute(parameter))
                ExceptionManager.ThrowCommandCannotBeExecuted();
            return true;
        }

        #endregion
    }
}