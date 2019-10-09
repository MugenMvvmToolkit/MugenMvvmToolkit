using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
#pragma warning disable 660,661
    public class NavigationCallbackType : EnumBase<NavigationCallbackType, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly NavigationCallbackType Showing = new NavigationCallbackType(1);
        public static readonly NavigationCallbackType Closing = new NavigationCallbackType(2);
        public static readonly NavigationCallbackType Close = new NavigationCallbackType(3);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected NavigationCallbackType()
        {
        }

        public NavigationCallbackType(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NavigationCallbackType? left, NavigationCallbackType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NavigationCallbackType? left, NavigationCallbackType? right)
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