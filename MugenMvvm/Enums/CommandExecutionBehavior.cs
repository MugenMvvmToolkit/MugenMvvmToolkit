using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using MugenMvvm.Attributes;
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

        public static readonly CommandExecutionBehavior None = new(0, null, null);
        public static readonly CommandExecutionBehavior CheckCanExecute = new(1, CheckCanExecuteIml, null);
        public static readonly CommandExecutionBehavior CheckCanExecuteThrow = new(2, CheckCanExecuteThrowImpl, null);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected CommandExecutionBehavior()
        {
        }

        public CommandExecutionBehavior(int value, Func<ICommand, object?, bool>? beforeExecute, Action<ICommand, object?>? afterExecute, string? name = null) : base(value, name)
        {
            _beforeExecute = beforeExecute;
            _afterExecute = afterExecute;
        }

        #endregion

        #region Methods

        public bool BeforeExecute(ICommand command, object? parameter) => _beforeExecute?.Invoke(command, parameter) ?? true;

        public void AfterExecute(ICommand command, object? parameter) => _afterExecute?.Invoke(command, parameter);

        private static bool CheckCanExecuteIml(ICommand command, object? parameter)
        {
            if (command.CanExecute(parameter))
                return true;
            (command as ICompositeCommand)?.RaiseCanExecuteChanged();
            return false;
        }

        private static bool CheckCanExecuteThrowImpl(ICommand command, object? parameter)
        {
            if (!command.CanExecute(parameter))
                ExceptionManager.ThrowCommandCannotBeExecuted();
            return true;
        }

        #endregion
    }
}