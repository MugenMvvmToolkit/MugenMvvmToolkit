using System;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        public static Exception EnumIsNotValid(Type type, object value)
        {
            return new ArgumentException(MessageConstants.EnumIsNotValidFormat2.Format(value, type), nameof(value));
        }

        internal static Exception CapacityLessThanCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, MessageConstants.CapacityShouldBeGreaterOrEqual);
        }

        internal static Exception IndexOutOfRangeCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, MessageConstants.IndexMustBeWithinBounds);
        }

        internal static Exception EnumOutOfRange(string paramName, Enum @enum)
        {
            return new ArgumentOutOfRangeException(paramName, MessageConstants.UnhandledEnumFormat1.Format(@enum));
        }

        internal static Exception CommandCannotBeExecuted()
        {
            return new InvalidOperationException(MessageConstants.CommandCannotBeExecutedString);
        }

        #endregion
    }
}