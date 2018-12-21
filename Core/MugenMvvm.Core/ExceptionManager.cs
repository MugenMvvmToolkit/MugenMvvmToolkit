using System;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        public static Exception EnumIsNotValid(Type type, object value)
        {
            var message = string.Format("'{0}' is not a valid in {1}", value, type);
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

        #endregion
    }
}