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
        public const int NoneValue = 0;
        public const int CheckCanExecuteValue = 1;
        public const int CheckCanExecuteThrowValue = 2;
        public static readonly CommandExecutionBehavior None = new(NoneValue, null, null);
        public static readonly CommandExecutionBehavior CheckCanExecute = new(CheckCanExecuteValue, CheckCanExecuteIml, null);
        public static readonly CommandExecutionBehavior CheckCanExecuteThrow = new(CheckCanExecuteThrowValue, CheckCanExecuteThrowImpl, null);

        private readonly Action<ICommand, object?>? _afterExecute;
        private readonly Func<ICommand, object?, bool>? _beforeExecute;

        public CommandExecutionBehavior(int value, Func<ICommand, object?, bool>? beforeExecute, Action<ICommand, object?>? afterExecute, string? name = null) : base(value, name)
        {
            _beforeExecute = beforeExecute;
            _afterExecute = afterExecute;
        }

        [Preserve(Conditional = true)]
        protected CommandExecutionBehavior()
        {
        }

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

        public bool BeforeExecute(ICommand command, object? parameter) => _beforeExecute?.Invoke(command, parameter) ?? true;

        public void AfterExecute(ICommand command, object? parameter) => _afterExecute?.Invoke(command, parameter);
    }
}