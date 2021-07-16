using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = InternalConstant.DataContractNamespace)]
    public abstract class EnumBase<TEnum, TValue> : IEnum, IHasId<TValue>, IComparable<TEnum?>, IEquatable<TEnum?>
        where TEnum : EnumBase<TEnum, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        private static Dictionary<TValue, TEnum> _enumerations = Init();
        private static readonly Dictionary<string, TEnum> EnumerationNamesField = new(StringComparer.OrdinalIgnoreCase);

        private static TEnum[]? _values;
        private string? _name;

#pragma warning disable CS8618
        //note serialization only
        protected EnumBase()
        {
        }
#pragma warning restore CS8618

        protected EnumBase(TValue value, string? name = null)
        {
            Value = value;
            _name = name;
            lock (_enumerations)
            {
                if (!_enumerations.ContainsKey(value))
                {
                    _enumerations[value] = (TEnum) this;
                    EnumerationNamesField[Name] = (TEnum) this;
                    _values = null;
                }
                else if (EnumBase.ThrowOnDuplicate)
                    ExceptionManager.ThrowDuplicateEnum(_enumerations[value], this);
            }
        }

        public static int Count => Enumerations.Count;

        [DataMember(Name = "_v")]
        public TValue Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            internal set;
        }

        [DataMember(Name = "_d")]
        public string Name
        {
            get => _name ??= Value?.ToString() ?? "";
            internal set => _name = value;
        }

        private static Dictionary<TValue, TEnum> Enumerations
        {
            get
            {
                if (_enumerations.Count == 0)
                    RuntimeHelpers.RunClassConstructor(typeof(TEnum).TypeHandle);
                return _enumerations;
            }
        }

        private static Dictionary<string, TEnum> EnumerationNames
        {
            get
            {
                if (EnumerationNamesField.Count == 0)
                    RuntimeHelpers.RunClassConstructor(typeof(TEnum).TypeHandle);
                return EnumerationNamesField;
            }
        }

        object IEnum.Value => BoxingExtensions.Box(Value);

        TValue IHasId<TValue>.Id => Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return EqualityComparer<TValue>.Default.Equals(left.Value, right.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right) => CompareTo(left, right) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right) => CompareTo(left, right) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right) => CompareTo(left, right) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right) => CompareTo(left, right) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator EnumBase<TEnum, TValue>(string value) => GetByName(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator EnumBase<TEnum, TValue>(TValue value) => Get(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TValue(EnumBase<TEnum, TValue> value) => value.Value;

        public static TEnum[] GetAll()
        {
            if (_values != null)
                return _values;
            lock (_enumerations)
            {
                return _values ??= Enumerations.Values.ToArray();
            }
        }

        public static TEnum Get(TValue value)
        {
            if (!TryGet(value, out var result))
                ExceptionManager.ThrowEnumIsNotValid(value);

            return result;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static TEnum? TryGet(TValue value, TEnum? defaultValue = null)
        {
            if (TryGet(value, out var result))
                return result;
            return defaultValue;
        }

        public static bool TryGet(TValue? value, [NotNullWhen(true)] out TEnum? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            lock (_enumerations)
            {
                return Enumerations.TryGetValue(value, out result);
            }
        }

        public static TEnum GetByName(string value, bool ignoreCase = false)
        {
            Should.NotBeNull(value, nameof(value));
            if (!TryGetByName(value, out var result, ignoreCase))
                ExceptionManager.ThrowEnumIsNotValid(value);

            return result;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static TEnum? TryGetByName(string? value, TEnum? defaultValue = null, bool ignoreCase = false)
        {
            if (TryGetByName(value, out var result, ignoreCase))
                return result;
            return defaultValue;
        }

        public static bool TryGetByName(string? value, [NotNullWhen(true)] out TEnum? result, bool ignoreCase = false)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            lock (_enumerations)
            {
                return EnumerationNames.TryGetValue(value, out result) && (ignoreCase || value.Equals(result.Name));
            }
        }

        public static void SetEnums(Dictionary<TValue, TEnum> enumerations)
        {
            Should.NotBeNull(enumerations, nameof(enumerations));
            _enumerations = enumerations;
            EnumerationNamesField.Clear();
            foreach (var enumeration in enumerations)
                EnumerationNamesField[enumeration.Value.Name] = enumeration.Value;
            _values = null;
        }

        public static void SetEnum(TValue value, TEnum enumeration)
        {
            Should.NotBeNull(enumeration, nameof(enumeration));
            lock (_enumerations)
            {
                _enumerations[value] = enumeration;
                EnumerationNamesField[enumeration.Name] = enumeration;
                _values = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override bool Equals(object? obj) => obj is TEnum e && Equals(e);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TEnum? other) => CompareTo(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TEnum? other) => ReferenceEquals(this, other) || !ReferenceEquals(other, null) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareTo(EnumBase<TEnum, TValue>? left, EnumBase<TEnum, TValue>? right)
        {
            if (ReferenceEquals(right, left))
                return 0;
            if (ReferenceEquals(right, null))
                return 1;
            if (ReferenceEquals(left, null))
                return -1;
            return Comparer<TValue>.Default.Compare(left.Value, right.Value);
        }

        private static Dictionary<TValue, TEnum> Init()
        {
            EnumBase.SetEnumProvider<TEnum, TValue>(GetAll, TryGet, TryGetByName);
            return new Dictionary<TValue, TEnum>();
        }
    }
}