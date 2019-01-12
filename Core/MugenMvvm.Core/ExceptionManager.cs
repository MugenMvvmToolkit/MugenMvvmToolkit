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

        internal static Exception DuplicateViewMapping(Type viewType, Type viewModelType, string? name)
        {
            return new InvalidOperationException(MessageConstants.DuplicateViewMappingFormat3.Format(viewType, viewModelType, name));
        }

        internal static Exception WrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            return new ArgumentException(MessageConstants.WrapperTypeShouldBeNonAbstractFormat1.Format(wrapperType), nameof(wrapperType));
        }

        internal static Exception WrapperTypeNotSupported(Type wrapperType)
        {
            return new ArgumentException(MessageConstants.WrapperTypeNotSupportedFormat1.Format(wrapperType), nameof(wrapperType));
        }

        internal static Exception DuplicateInterface(string itemName, string interfaceName, Type type)
        {
            return new InvalidOperationException(MessageConstants.DuplicateInterfaceFormat3.Format(itemName, interfaceName, type));
        }

        internal static Exception IntOutOfRangeCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, MessageConstants.IntOutOfRangeCollection);
        }

        internal static Exception PresenterCannotShowRequest(string request)
        {
            return new ArgumentException(MessageConstants.PresenterCannotShowRequestFormat1.Format(request));
        }

        internal static Exception PresenterInvalidRequest(string request)
        {
            return new ArgumentException(MessageConstants.PresenterCannotHandleRequestFormat1.Format(request));
        }

        internal static Exception NavigatingResultHasCallback()
        {
            return new InvalidOperationException(MessageConstants.NavigatingResultHasCallback);
        }

        internal static Exception ObjectInitialized(string objectName, object obj, string? hint = null)
        {
            string typeName = obj == null ? string.Empty : obj.GetType().FullName;
            return new InvalidOperationException(MessageConstants.ObjectInitializedFormat3.Format(objectName, typeName, hint));
        }

        internal static Exception ViewNotFound(Type viewModelType, Type? viewType = null)
        {
            string viewName = viewType == null ? "view" : viewType.FullName;
            return new InvalidOperationException(MessageConstants.ViewNotFoundFormat2.Format(viewName, viewModelType));
        }

        internal static Exception IoCCannotFindBinding(Type service)
        {
            return new InvalidOperationException(MessageConstants.IoCCannotFindBindingFormat1.Format(service));
        }

        internal static Exception IoCCyclicalDependency(Type service)
        {
            return new InvalidOperationException(MessageConstants.IoCCyclicalDependencyFormat1.Format(service));
        }

        internal static Exception IoCCannotFindConstructor(Type service)
        {
            return new InvalidOperationException(MessageConstants.IoCCannotFindConstructorFormat1.Format(service));
        }

        internal static Exception IoCMoreThatOneBinding(Type service)
        {
            return new InvalidOperationException(MessageConstants.IoCMoreThatOneBindingFormat1.Format(service));
        }

        #endregion
    }
}