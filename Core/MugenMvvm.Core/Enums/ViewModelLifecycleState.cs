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
    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly ViewModelLifecycleState Created = new ViewModelLifecycleState(nameof(Created));
        public static readonly ViewModelLifecycleState Initializing = new ViewModelLifecycleState(nameof(Initializing));
        public static readonly ViewModelLifecycleState Initialized = new ViewModelLifecycleState(nameof(Initialized));
        public static readonly ViewModelLifecycleState Disposing = new ViewModelLifecycleState(nameof(Disposing));
        public static readonly ViewModelLifecycleState Disposed = new ViewModelLifecycleState(nameof(Disposed));
        public static readonly ViewModelLifecycleState Finalized = new ViewModelLifecycleState(nameof(Finalized));
        public static readonly ViewModelLifecycleState Preserving = new ViewModelLifecycleState(nameof(Preserving));
        public static readonly ViewModelLifecycleState Preserved = new ViewModelLifecycleState(nameof(Preserved));
        public static readonly ViewModelLifecycleState Restoring = new ViewModelLifecycleState(nameof(Restoring));
        public static readonly ViewModelLifecycleState Restored = new ViewModelLifecycleState(nameof(Restored));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected ViewModelLifecycleState()
        {
        }

        public ViewModelLifecycleState(string value)
            : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ViewModelLifecycleState? left, ViewModelLifecycleState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ViewModelLifecycleState? left, ViewModelLifecycleState? right)
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