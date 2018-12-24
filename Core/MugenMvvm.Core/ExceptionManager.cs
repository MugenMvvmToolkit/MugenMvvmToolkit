using System;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        public const string StaticDelegateCannotBeWeak = "The static delegate cannot be converted to weak delegate.";
        public const string AnonymousDelegateCannotBeWeak = "The anonymous delegate cannot be converted to weak delegate.";

        #region Methods

        public static Exception EnumIsNotValid(Type type, object value)
        {
            var message = $"'{value}' is not a valid in {type}";
            return new ArgumentException(message, nameof(value));
        }

        internal static Exception CapacityLessThanCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, "The Capacity should be greater or equal than collection.");
        }

        internal static Exception IndexOutOfRangeCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, "Index must be within the bounds of the collection.");
        }

        internal static Exception EnumOutOfRange(string paramName, Enum @enum)
        {
            return new ArgumentOutOfRangeException(paramName, $"Unhandled enum - '{@enum}'");
        }

        #endregion
    }
}