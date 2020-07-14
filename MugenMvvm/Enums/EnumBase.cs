using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Constants;

namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    public abstract class EnumBase<TEnumeration, TValue> : IComparable<TEnumeration?>, IEquatable<TEnumeration?>
        where TEnumeration : EnumBase<TEnumeration, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        #region Fields

        private string? _name;
        private static Dictionary<TValue, TEnumeration> _enumerations = new Dictionary<TValue, TEnumeration>();

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
                _enumerations[value] = (TEnumeration)this;
        }

        protected EnumBase(TValue value)
            : this(value, null)
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "_d")]
        public string Name
        {
            get => _name ??= Value?.ToString() ?? string.Empty;
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

        public int CompareTo(TEnumeration? other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            return Value.CompareTo(other.Value);
        }

        public bool Equals(TEnumeration? other)
        {
            return !ReferenceEquals(other, null) && Equals(other.Value);
        }

        #endregion

        #region Methods

        protected abstract bool Equals(TValue value);

        public sealed override bool Equals(object? obj)
        {
            return Equals(obj as TEnumeration);
        }

        public sealed override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return HashCode.Combine(Value);
        }

        public sealed override string ToString()
        {
            return Name;
        }

        public static bool operator ==(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            return !(left == right);
        }

        public static explicit operator EnumBase<TEnumeration, TValue>(TValue value)
        {
            return Parse(value);
        }

        public static ICollection<TEnumeration> GetAll()
        {
            return Enumerations.Values;
        }

        public static bool TryParse([AllowNull]TValue value, [NotNullWhen(true)] out TEnumeration? result)
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

        public static TEnumeration FromValueOrDefault(TValue value, TEnumeration defaultValue)
        {
            if (TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        public static void SetEnumerations(Dictionary<TValue, TEnumeration> enumerations)
        {
            Should.NotBeNull(enumerations, nameof(enumerations));
            _enumerations = enumerations;
        }

        public static void SetEnum(TValue value, TEnumeration enumeration)
        {
            _enumerations[value] = enumeration;
        }

        #endregion
    }
}