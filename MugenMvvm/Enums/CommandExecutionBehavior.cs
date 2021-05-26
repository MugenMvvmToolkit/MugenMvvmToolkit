using System;
using System.Runtime.Serialization;
using System.Threading;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;

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

        private readonly Action<ICompositeCommand, object?, CancellationToken, IReadOnlyMetadataContext?>? _afterExecute;
        private readonly Func<ICompositeCommand, object?, CancellationToken, IReadOnlyMetadataContext?, bool>? _beforeExecute;

        public CommandExecutionBehavior(int value, Func<ICompositeCommand, object?, CancellationToken, IReadOnlyMetadataContext?, bool>? beforeExecute,
            Action<ICompositeCommand, object?, CancellationToken, IReadOnlyMetadataContext?>? afterExecute, string? name = null) : base(value, name)
        {
            _beforeExecute = beforeExecute;
            _afterExecute = afterExecute;
        }

        [Preserve(Conditional = true)]
        protected CommandExecutionBehavior()
        {
        }

        private static bool CheckCanExecuteIml(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (command.CanExecute(parameter, metadata))
                return true;
            command.RaiseCanExecuteChanged();
            return false;
        }

        private static bool CheckCanExecuteThrowImpl(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!command.CanExecute(parameter, metadata))
                ExceptionManager.ThrowCommandCannotBeExecuted();
            return true;
        }

        public bool BeforeExecute(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            _beforeExecute?.Invoke(command, parameter, cancellationToken, metadata) ?? true;

        public void AfterExecute(ICompositeCommand command, object? parameter, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            _afterExecute?.Invoke(command, parameter, cancellationToken, metadata);
    }
}