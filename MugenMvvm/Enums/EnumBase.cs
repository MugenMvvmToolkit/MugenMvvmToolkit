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
    public abstract class EnumBase<TEnumeration, TValue> : IEnum, IHasId<TValue>, IComparable<TEnumeration?>, IEquatable<TEnumeration?>
        where TEnumeration : EnumBase<TEnumeration, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        #region Fields

        private string? _name;
        private static Dictionary<TValue, TEnumeration> _enumerations = new Dictionary<TValue, TEnumeration>();
        private static TEnumeration[]? _values;

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
                _enumerations[value] = (TEnumeration) this;
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

        private static Dictionary<TValue, TEnumeration> Enumerations
        {
            get
            {
                if (_enumerations.Count == 0)
                    RuntimeHelpers.RunClassConstructor(typeof(TEnumeration).TypeHandle);
                return _enumerations;
            }
        }

        #endregion

        #region Implementation of interfaces

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(TEnumeration? other)
        {
            if (ReferenceEquals(other, this))
                return 0;
            if (ReferenceEquals(other, null))
                return 1;
            return Comparer<TValue>.Default.Compare(Value, other.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TEnumeration? other) => ReferenceEquals(this, other) || !ReferenceEquals(other, null) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return EqualityComparer<TValue>.Default.Equals(left.Value, right.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right) => !(left == right);

        public static explicit operator EnumBase<TEnumeration, TValue>(TValue value) => Parse(value);

        public static explicit operator TValue(EnumBase<TEnumeration, TValue> value) => value.Value;

        public static TEnumeration[] GetAll() => _values ??= Enumerations.Values.ToArray();

        public static TEnumeration TryParse(TValue value, TEnumeration defaultValue)
        {
            if (TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        public static bool TryParse([AllowNull] TValue value, [NotNullWhen(true)] out TEnumeration? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            return Enumerations.TryGetValue(value, out result);
        }

        public static TEnumeration Parse(TValue value)
        {
            if (!TryParse(value, out var result))
                ExceptionManager.ThrowEnumIsNotValid(value);

            return result;
        }

        public static void SetEnums(Dictionary<TValue, TEnumeration> enumerations)
        {
            Should.NotBeNull(enumerations, nameof(enumerations));
            _enumerations = enumerations;
            _values = null;
        }

        public static void SetEnum(TValue value, TEnumeration enumeration)
        {
            _enumerations[value] = enumeration;
            _values = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override bool Equals(object? obj) => obj is TEnumeration e && Equals(e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public sealed override int GetHashCode() => HashCode.Combine(Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override string ToString() => Name;

        #endregion
    }
}