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

        public static readonly ApplicationLifecycleState Initializing = new ApplicationLifecycleState(nameof(Initializing));
        public static readonly ApplicationLifecycleState Initialized = new ApplicationLifecycleState(nameof(Initialized));
        public static readonly ApplicationLifecycleState Activating = new ApplicationLifecycleState(nameof(Activating));
        public static readonly ApplicationLifecycleState Activated = new ApplicationLifecycleState(nameof(Activated));
        public static readonly ApplicationLifecycleState Deactivating = new ApplicationLifecycleState(nameof(Deactivating));
        public static readonly ApplicationLifecycleState Deactivated = new ApplicationLifecycleState(nameof(Deactivated));

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
        public static bool operator !=(ApplicationLifecycleState? left, ApplicationLifecycleState? right) => !(left == right);

        protected override bool Equals(string value) => Value.Equals(value);

        #endregion
    }
}