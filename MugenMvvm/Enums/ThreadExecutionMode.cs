using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class ThreadExecutionMode : EnumBase<ThreadExecutionMode, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly ThreadExecutionMode Main = new ThreadExecutionMode(1);
        public static readonly ThreadExecutionMode MainAsync = new ThreadExecutionMode(2);
        public static readonly ThreadExecutionMode Background = new ThreadExecutionMode(3);
        public static readonly ThreadExecutionMode Current = new ThreadExecutionMode(4);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ThreadExecutionMode()
        {
        }

        public ThreadExecutionMode(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ThreadExecutionMode? left, ThreadExecutionMode? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ThreadExecutionMode? left, ThreadExecutionMode? right)
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