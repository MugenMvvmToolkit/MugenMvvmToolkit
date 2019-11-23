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
    public class IocParameterType : EnumBase<IocParameterType, int>
#pragma warning restore 660,661
    {
        #region Fields

        public static readonly IocParameterType Constructor = new IocParameterType(1);
        public static readonly IocParameterType Property = new IocParameterType(2);

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        protected IocParameterType()
        {
        }

        public IocParameterType(int value) : base(value)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(IocParameterType? left, IocParameterType? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Value == right.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(IocParameterType? left, IocParameterType? right)
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