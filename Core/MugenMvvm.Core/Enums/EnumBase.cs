using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MugenMvvm.Constants;

#pragma warning disable CS8618
namespace MugenMvvm.Enums
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class EnumBase<TEnumeration, TValue> : IComparable<TEnumeration>, IEquatable<TEnumeration>
        where TEnumeration : EnumBase<TEnumeration, TValue>
        where TValue : IComparable<TValue>, IEquatable<TValue>
    {
        #region Fields

        private string? _displayName;
        private TValue _value;

        private static Dictionary<TValue, TEnumeration> _enumerations;

        #endregion

        #region Constructors

        //note serialization only
        protected EnumBase()
        {
        }

        protected EnumBase(TValue value, string? displayName)
        {
            _value = value;
            _displayName = displayName;
        }

        protected EnumBase(TValue value)
            : this(value, null)
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "_d")]
        public string DisplayName
        {
            get
            {
                if (_displayName == null)
                    _displayName = Value?.ToString() ?? string.Empty;
                return _displayName;
            }
            internal set => _displayName = value;
        }

        [DataMember(Name = "_v")]
        public TValue Value
        {
            get => _value;
            internal set => _value = value;
        }

        #endregion

        #region Implementation of interfaces

        public int CompareTo(TEnumeration other)
        {
            if (other == null)
                return 1;
            return Value.CompareTo(other.Value);
        }

        public bool Equals(TEnumeration other)
        {
            return other != null && Value.Equals(other.Value);
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return Equals(obj as TEnumeration);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public sealed override string ToString()
        {
            return DisplayName;
        }

        public static bool operator ==(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            return !Equals(left, right);
        }

        public static ICollection<TEnumeration> GetAll()
        {
            return GetEnumerations().Values;
        }

        public static bool TryParse(TValue value, out TEnumeration result)
        {
            if (value == null)
            {
                result = default!;
                return false;
            }

            return GetEnumerations().TryGetValue(value, out result);
        }

        public static TEnumeration FromValue(TValue value)
        {
            if (!TryParse(value, out var result))
                ExceptionManager.ThrowEnumIsNotValid(value);

            return result;
        }

        public static TEnumeration FromValueOrDefault(TValue value, TEnumeration defaultValue)
        {
            if (!TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        public static void SetAllEnumerations(Dictionary<TValue, TEnumeration> enumerations)
        {
            Should.NotBeNull(enumerations, nameof(enumerations));
            _enumerations = enumerations;
        }

        private static Dictionary<TValue, TEnumeration> GetEnumerations()
        {
            if (_enumerations == null)
            {
                _enumerations = typeof(TEnumeration)
                    .GetFieldsUnified(MemberFlags.StaticPublic)
                    .Where(info => typeof(TEnumeration).IsAssignableFromUnified(info.FieldType))
                    .Select(info => info.GetValue(null))
                    .Cast<TEnumeration>()
                    .ToDictionary(enumeration => enumeration.Value);
            }

            return _enumerations;
        }

        #endregion
    }
}
#pragma warning restore CS8618