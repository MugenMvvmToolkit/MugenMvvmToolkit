// ReSharper disable MethodOverloadWithOptionalParameter
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Value<T>(this EnumFlags<T> flags, byte _ = 0) where T : FlagsEnumBase<T, byte> => (byte) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Value<T>(this EnumFlags<T> flags, sbyte _ = 0) where T : FlagsEnumBase<T, sbyte> => (sbyte) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Value<T>(this EnumFlags<T> flags, short _ = 0) where T : FlagsEnumBase<T, short> => (short) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Value<T>(this EnumFlags<T> flags, ushort _ = 0) where T : FlagsEnumBase<T, ushort> => (ushort) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Value<T>(this EnumFlags<T> flags) where T : FlagsEnumBase<T, int> => (int) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Value<T>(this EnumFlags<T> flags, uint _ = 0) where T : FlagsEnumBase<T, uint> => (uint) flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Value<T>(this EnumFlags<T> flags, long _ = 0) where T : FlagsEnumBase<T, long> => flags.Flags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Value<T>(this EnumFlags<T> flags, ulong _ = 0) where T : FlagsEnumBase<T, ulong> => (ulong) flags.Flags;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, byte _ = 0) where T : FlagsEnumBase<T, byte> => GetFlags<T, byte>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, sbyte _ = 0) where T : FlagsEnumBase<T, sbyte> => GetFlags<T, sbyte>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, short _ = 0) where T : FlagsEnumBase<T, short> => GetFlags<T, short>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, ushort _ = 0) where T : FlagsEnumBase<T, ushort> => GetFlags<T, ushort>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags) where T : FlagsEnumBase<T, int> => GetFlags<T, int>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, uint _ = 0) where T : FlagsEnumBase<T, uint> => GetFlags<T, uint>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, long _ = 0) where T : FlagsEnumBase<T, long> => GetFlags<T, long>(flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<T, List<T>> GetFlags<T>(this EnumFlags<T> flags, ulong _ = 0) where T : FlagsEnumBase<T, ulong> => GetFlags<T, ulong>(flags);

        private static ItemOrList<T, List<T>> GetFlags<T, TValue>(this EnumFlags<T> flags)
            where T : FlagsEnumBase<T, TValue>
            where TValue : IComparable<TValue>, IEquatable<TValue>, IConvertible
        {
            var editor = ItemOrListEditor.Get<T>();
            foreach (var value in FlagsEnumBase<T, TValue>.GetAll())
            {
                if (flags.HasFlag(value.FlagValue))
                    editor.Add(value);
            }

            return editor.ToItemOrList<List<T>>();
        }

#if SPAN_API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, byte _ = 0) where T : FlagsEnumBase<T, byte> => GetFlags<T, byte>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, sbyte _ = 0) where T : FlagsEnumBase<T, sbyte> => GetFlags<T, sbyte>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, short _ = 0) where T : FlagsEnumBase<T, short> => GetFlags<T, short>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, ushort _ = 0) where T : FlagsEnumBase<T, ushort> => GetFlags<T, ushort>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values) where T : FlagsEnumBase<T, int> => GetFlags<T, int>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, uint _ = 0) where T : FlagsEnumBase<T, uint> => GetFlags<T, uint>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, long _ = 0) where T : FlagsEnumBase<T, long> => GetFlags<T, long>(flags, values);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlags<T>(this EnumFlags<T> flags, Span<T> values, ulong _ = 0) where T : FlagsEnumBase<T, ulong> => GetFlags<T, ulong>(flags, values);

        private static int GetFlags<T, TValue>(this EnumFlags<T> flags, Span<T> values)
            where T : FlagsEnumBase<T, TValue>
            where TValue : IComparable<TValue>, IEquatable<TValue>, IConvertible
        {
            int count = 0;
            foreach (var value in FlagsEnumBase<T, TValue>.GetAll())
            {
                if (flags.HasFlag(value))
                    values[count++] = value;
            }

            return count;
        }
#endif

        #endregion
    }
}