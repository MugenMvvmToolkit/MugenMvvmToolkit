using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct NullableKey<T> : IEquatable<NullableKey<T>>
    {
        public readonly T? Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NullableKey(T? value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NullableKey<T>(T? value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T?(NullableKey<T> value) => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NullableKey<T> left, NullableKey<T> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NullableKey<T> left, NullableKey<T> right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NullableKey<T> other) => EqualityComparer<T?>.Default.Equals(Value, other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is NullableKey<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => EqualityComparer<T?>.Default.GetHashCode(Value!);
    }
}