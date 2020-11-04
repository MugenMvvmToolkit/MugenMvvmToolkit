using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Enums
{
    public abstract class FlagsEnumBase<TEnum, TValue> : EnumBase<TEnum, TValue>, IFlagsEnum
        where TEnum : FlagsEnumBase<TEnum, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>, IConvertible
    {
        #region Constructors

        protected FlagsEnumBase()
        {
        }

        protected FlagsEnumBase(TValue value, string? name)
            : this(value, name, ConvertValue(value))
        {
        }

        protected FlagsEnumBase(TValue value)
            : this(value, ConvertValue(value))
        {
        }

        protected FlagsEnumBase(TValue value, string? name, long flagValue)
            : base(value, name)
        {
            FlagValue = flagValue;
        }

        protected FlagsEnumBase(TValue value, long flagValue) : base(value)
        {
            FlagValue = flagValue;
        }

        #endregion

        #region Properties

        [DataMember(Name = "_f")]
        public long FlagValue { get; internal set; }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EnumFlags<TEnum>(FlagsEnumBase<TEnum, TValue> value) => new EnumFlags<TEnum>(value.FlagValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator |(FlagsEnumBase<TEnum, TValue> left, FlagsEnumBase<TEnum, TValue> right) => new EnumFlags<TEnum>(left.FlagValue | right.FlagValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator &(FlagsEnumBase<TEnum, TValue> left, FlagsEnumBase<TEnum, TValue> right) => new EnumFlags<TEnum>(left.FlagValue & right.FlagValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator ~(FlagsEnumBase<TEnum, TValue> value) => new EnumFlags<TEnum>(~value.FlagValue);

        private static long ConvertValue(TValue value)
        {
            if (typeof(TValue) == typeof(int))
                return Cast<int>(value);
            if (typeof(TValue) == typeof(byte))
                return Cast<byte>(value);
            if (typeof(TValue) == typeof(long))
                return Cast<long>(value);
            if (typeof(TValue) == typeof(ulong))
                return (long) Cast<ulong>(value);
            return Convert.ToInt64(value);
        }

        private static T Cast<T>(TValue value) => MugenExtensions.CastGeneric<TValue, T>(value);

        #endregion
    }
}