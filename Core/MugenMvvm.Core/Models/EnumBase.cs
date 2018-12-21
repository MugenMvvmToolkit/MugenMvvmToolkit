using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MugenMvvm.Models
{
    public class EnumBase<TEnumeration, TValue> : IComparable<TEnumeration?>, IEquatable<TEnumeration?>
        where TEnumeration : EnumBase<TEnumeration, TValue>
        where TValue : IComparable
    {
        #region Fields

        private static Dictionary<TValue, TEnumeration> _enumerations;

        #endregion

        #region Constructors

        protected EnumBase(TValue value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        protected EnumBase(TValue value)
            : this(value, value.ToString())
        {
        }

        #endregion

        #region Properties

        public string DisplayName { get; }

        public TValue Value { get; }

        private static Dictionary<TValue, TEnumeration> Enumerations
        {
            get
            {
                if (_enumerations == null)
                    _enumerations = GetEnumerations();
                return _enumerations;
            }
        }

        #endregion

        #region Implementation of interfaces

        public int CompareTo(TEnumeration? other)
        {
            if (other == null)
                return 1;
            return Value.CompareTo(other.Value);
        }

        public bool Equals(TEnumeration? other)
        {
            return other != null! && Value.Equals(other.Value);
        }

        #endregion

        #region Methods

        public static TEnumeration FromValue(TValue value)
        {
            if (!TryParse(value, out var result))
                throw ExceptionManager.EnumIsNotValid(typeof(TEnumeration), value);
            return result;
        }

        public static TEnumeration FromValueOrDefault(TValue value, TEnumeration defaultValue)
        {
            if (!TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        public static ICollection<TEnumeration> GetAll()
        {
            return Enumerations.Values;
        }

        public static bool operator ==(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnumBase<TEnumeration, TValue>? left, EnumBase<TEnumeration, TValue>? right)
        {
            return !Equals(left, right);
        }

        public static bool TryParse(TValue value, out TEnumeration result)
        {
            if (value == null)
            {
                result = default!;
                return false;
            }

            return Enumerations.TryGetValue(value, out result);
        }

        private static Dictionary<TValue, TEnumeration> GetEnumerations()
        {
            var enumerationType = typeof(TEnumeration);
            return enumerationType
                .GetFieldsUnified(MemberFlags.StaticPublic)
                .Where(info => enumerationType.GetTypeInfo().IsAssignableFrom(info.FieldType.GetTypeInfo()))
                .Select(info => info.GetValue(null))
                .Cast<TEnumeration>()
                .ToDictionary(enumeration => enumeration.Value);
        }

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

        #endregion
    }
}