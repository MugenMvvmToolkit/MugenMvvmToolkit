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
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public abstract class EnumBase<TEnum, TValue> : IEnum, IHasId<TValue>, IComparable<TEnum?>, IEquatable<TEnum?>
        where TEnum : EnumBase<TEnum, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        #region Fields

        private string? _name;
        private static Dictionary<TValue, TEnum> _enumerations = Init();
        private static readonly Dictionary<string, TEnum> EnumerationNamesField = new(StringComparer.OrdinalIgnoreCase);
        private static TEnum[]? _values;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        //note serialization only
        protected EnumBase()
        {
        }
#pragma warning restore CS8618

        protected EnumBase(TValue value, string? name)
        {
            Value = value;
            _name = name;
            if (!_enumerations.ContainsKey(value))
            {
                _enumerations[value] = (TEnum) this;
                EnumerationNamesField[Name] = (TEnum) this;
                _values = null;
            }
        }

        protected EnumBase(TValue value)
            : this(value, null)
        {
        }

        #endregion

        #region Properties

        TValue IHasId<TValue>.Id => Value;

        object IEnum.Value => BoxingExtensions.Box(Value);

        [DataMember(Name = "_d")]
        public string Name
        {
            get => _name ??= Value?.ToString() ?? "";
            internal set => _name = value;
        }

        [DataMember(Name = "_v")]
        public TValue Value { get; internal set; }

        public static int Count => Enumerations.Count;

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

        #endregion

        #region Implementation of interfaces

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TEnum? other) => CompareTo(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TEnum? other) => ReferenceEquals(this, other) || !ReferenceEquals(other, null) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);

        #endregion

        #region Methods

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum[] GetAll() => _values ??= Enumerations.Values.ToArray();

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

        public static bool TryGet([AllowNull] TValue value, [NotNullWhen(true)] out TEnum? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            return Enumerations.TryGetValue(value, out result);
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

            return EnumerationNames.TryGetValue(value, out result) && (ignoreCase || value.Equals(result.Name));
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
            _enumerations[value] = enumeration;
            EnumerationNamesField[enumeration.Name] = enumeration;
            _values = null;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override bool Equals(object? obj) => obj is TEnum e && Equals(e);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Name;

        #endregion
    }
}