using System;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        public static void ThrowEnumIsNotValid(object value)
        {
            throw new ArgumentException(MessageConstants.EnumIsNotValidFormat2.Format(value, value.GetType()), nameof(value));
        }

        internal static void ThrowCapacityLessThanCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, MessageConstants.CapacityShouldBeGreaterOrEqual);
        }

        internal static void ThrowIndexOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, MessageConstants.IndexMustBeWithinBounds);
        }

        internal static void ThrowEnumOutOfRange(string paramName, object @enum)
        {
            throw new ArgumentOutOfRangeException(paramName, MessageConstants.UnhandledEnumFormat1.Format(@enum));
        }

        internal static void ThrowCommandCannotBeExecuted()
        {
            throw new InvalidOperationException(MessageConstants.CommandCannotBeExecutedString);
        }

        internal static void ThrowWrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            throw new ArgumentException(MessageConstants.WrapperTypeShouldBeNonAbstractFormat1.Format(wrapperType), nameof(wrapperType));
        }

        internal static void ThrowWrapperTypeNotSupported(Type wrapperType)
        {
            throw new ArgumentException(MessageConstants.WrapperTypeNotSupportedFormat1.Format(wrapperType), nameof(wrapperType));
        }

        internal static void ThrowIntOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, MessageConstants.IntOutOfRangeCollection);
        }

        internal static void ThrowPresenterCannotShowRequest(IReadOnlyMetadataContext request)
        {
            throw new ArgumentException(MessageConstants.PresenterCannotShowRequestFormat1.Format(request.Dump()));
        }

        internal static void ThrowPresenterInvalidRequest(IReadOnlyMetadataContext request, IReadOnlyMetadataContext response)
        {
            throw new ArgumentException(MessageConstants.PresenterCannotHandleRequestFormat1.Format(request.Dump() + response.Dump()));
        }

        internal static void ThrowNavigatingResultHasCallback()
        {
            throw new InvalidOperationException(MessageConstants.NavigatingResultHasCallback);
        }

        internal static void ThrowViewNotFound(Type viewModelType, Type? viewType = null)
        {
            var viewName = viewType == null ? "view" : viewType.FullName;
            throw new InvalidOperationException(MessageConstants.ViewNotFoundFormat2.Format(viewName, viewModelType));
        }

        internal static void ThrowCannotFindConstructor(Type service)
        {
            throw new InvalidOperationException(MessageConstants.CannotFindConstructorFormat1.Format(service));
        }

        internal static void ThrowIoCCannotFindBinding(Type service)
        {
            throw new InvalidOperationException(MessageConstants.IoCCannotFindBindingFormat1.Format(service));
        }

        internal static void ThrowIoCCyclicalDependency(Type service)
        {
            throw new InvalidOperationException(MessageConstants.IoCCyclicalDependencyFormat1.Format(service));
        }

        internal static void ThrowIoCMoreThatOneBinding(Type service)
        {
            throw new InvalidOperationException(MessageConstants.IoCMoreThatOneBindingFormat1.Format(service));
        }

        internal static void ThrowCannotGetViewModel(Type viewModelType)
        {
            throw new InvalidOperationException(MessageConstants.CannotGetViewModelFormat1.Format(viewModelType));
        }

        internal static void ThrowObjectDisposed(object item)
        {
            throw new ObjectDisposedException(item.GetType().FullName, MessageConstants.ObjectDisposedFormat1.Format(item.GetType()));
        }

        internal static void ThrowObjectNotInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(MessageConstants.ObjectNotInitializedFormat2.Format(obj.GetType().Name, hint));
        }

        internal static void ThrowObjectInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(MessageConstants.ObjectInitializedFormat3.Format(obj, obj?.GetType(), hint));
        }

        #endregion
    }
}