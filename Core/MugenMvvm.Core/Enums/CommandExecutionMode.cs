using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;

namespace MugenMvvm.Enums
{
    public class CommandExecutionMode : EnumBase<CommandExecutionMode, int>
    {
        #region Fields

        public static readonly CommandExecutionMode None = new CommandExecutionMode(1);//todo review
        public static readonly CommandExecutionMode CanExecuteBeforeExecute = new CommandExecutionMode(2);
        public static readonly CommandExecutionMode CanExecuteBeforeExecuteWithException = new CommandExecutionMode(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected CommandExecutionMode()
        {
        }

        public CommandExecutionMode(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(CommandExecutionMode? left, CommandExecutionMode? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(CommandExecutionMode? left, CommandExecutionMode? right)
        {
            return !(left == right);
        }

        protected override bool Equals(int value)
        {
            return Value == value;
        }

        #endregion
    }
}