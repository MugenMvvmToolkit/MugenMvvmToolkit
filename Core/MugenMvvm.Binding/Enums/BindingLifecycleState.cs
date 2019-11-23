using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
#pragma warning disable 660,661
    public class BindingLifecycleState : EnumBase<BindingLifecycleState, string>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly BindingLifecycleState Created = new BindingLifecycleState(nameof(Created));
        public static readonly BindingLifecycleState Initialized = new BindingLifecycleState(nameof(Initialized));
        public static readonly BindingLifecycleState Disposed = new BindingLifecycleState(nameof(Disposed));

        #endregion

        #region Constructors
        
        [Preserve(Conditional = true)]
        protected BindingLifecycleState()
        {
        }

        public BindingLifecycleState(string value)
            : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BindingLifecycleState? left, BindingLifecycleState? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BindingLifecycleState? left, BindingLifecycleState? right)
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