using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct Optional<T> : IEquatable<Optional<T>>
    {
        public readonly bool HasValue;
        private readonly T? _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional(T? value)
        {
            _value = value;
            HasValue = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional(T? value, bool hasValue)
        {
            _value = value;
            HasValue = hasValue;
        }

        public static Optional<T> None
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => default;
        }

        public static Optional<T> Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(default);
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasNonNullValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => HasValue && Value != null;
        }

        public T? Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!HasValue)
                    ExceptionManager.ThrowOptionalNoValue();
                return _value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Optional<T>(T? value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T?(Optional<T> value) => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Optional<T> Cast<TFrom>(TFrom? value) => new Optional<TFrom>(value).Cast<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetValueOrDefault() => _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetValueOrDefault(T defaultValue)
        {
            if (HasValue)
                return _value;
            return defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<TTo> Cast<TTo>()
        {
            if (!HasValue)
                return default;
            return typeof(TTo) == typeof(VoidResult) ? Optional<TTo>.Default : new Optional<TTo>(MugenExtensions.CastGeneric<T?, TTo>(Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Optional<VoidResult> AsVoid() => HasValue ? Optional<VoidResult>.Default : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Optional<T> other) => HasValue == other.HasValue && EqualityComparer<T?>.Default.Equals(_value, other._value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                return (HasValue.GetHashCode() * 397) ^ EqualityComparer<T?>.Default.GetHashCode(_value!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => HasValue ? _value?.ToString()! : "none";
    }
}