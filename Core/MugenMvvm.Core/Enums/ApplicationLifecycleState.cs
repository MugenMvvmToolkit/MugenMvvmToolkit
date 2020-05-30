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
    public class ApplicationLifecycleState : EnumBase<ApplicationLifecycleState, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly ApplicationLifecycleState Active = new ApplicationLifecycleState(nameof(Active));
        public static readonly ApplicationLifecycleState Background = new ApplicationLifecycleState(nameof(Background));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ApplicationLifecycleState()
        {
        }

        public ApplicationLifecycleState(string value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ApplicationLifecycleState? left, ApplicationLifecycleState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ApplicationLifecycleState? left, ApplicationLifecycleState? right)
        {
            return !(left == right);
        }

        protected override bool Equals(string value)
        {
            return Value.Equals(value);
        }

        #endregion
    }
}