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

        protected FlagsEnumBase(TValue value, string? name = null, long? flag = null)
            : base(value, name)
        {
            Flag = flag ?? ConvertValue(value);
        }

        #endregion

        #region Properties

        [DataMember(Name = "_f")]
        public long Flag { get; internal set; }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EnumFlags<TEnum>(FlagsEnumBase<TEnum, TValue>? value) => ReferenceEquals(value, null) ? default : new EnumFlags<TEnum>(value.Flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator |(FlagsEnumBase<TEnum, TValue> left, FlagsEnumBase<TEnum, TValue> right) => new(left.Flag | right.Flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator &(FlagsEnumBase<TEnum, TValue> left, FlagsEnumBase<TEnum, TValue> right) => new(left.Flag & right.Flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumFlags<TEnum> operator ~(FlagsEnumBase<TEnum, TValue> value) => new(~value.Flag);

        private static long ConvertValue(TValue value)
        {
            if (typeof(TValue) == typeof(int))
                return Cast<int>(value);
            if (typeof(TValue) == typeof(uint))
                return Cast<uint>(value);
            if (typeof(TValue) == typeof(short))
                return Cast<short>(value);
            if (typeof(TValue) == typeof(ushort))
                return Cast<ushort>(value);
            if (typeof(TValue) == typeof(byte))
                return Cast<byte>(value);
            if (typeof(TValue) == typeof(sbyte))
                return Cast<sbyte>(value);
            if (typeof(TValue) == typeof(long))
                return Cast<long>(value);
            if (typeof(TValue) == typeof(ulong))
                return (long) Cast<ulong>(value);
            return Convert.ToInt64(value);
        }

        private static T Cast<T>(TValue value) => MugenExtensions.CastGeneric<TValue, T>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumFlags<TEnum> AsFlags() => new(Flag);

        #endregion
    }
}