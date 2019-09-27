using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using static MugenMvvm.Constants.MessageConstants;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        public static void ThrowEnumIsNotValid(object value)
        {
            throw new ArgumentException(EnumIsNotValidFormat2.Format(value, value.GetType()), nameof(value));
        }

        public static void ThrowCapacityLessThanCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, CapacityShouldBeGreaterOrEqual);
        }

        public static void ThrowIndexOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, IndexMustBeWithinBounds);
        }

        public static void ThrowEnumOutOfRange(string paramName, object @enum)
        {
            throw new ArgumentOutOfRangeException(paramName, UnhandledEnumFormat1.Format(@enum));
        }

        public static void ThrowCommandCannotBeExecuted()
        {
            throw new InvalidOperationException(CommandCannotBeExecutedString);
        }

        public static void ThrowWrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            throw new ArgumentException(WrapperTypeShouldBeNonAbstractFormat1.Format(wrapperType), nameof(wrapperType));
        }

        public static void ThrowWrapperTypeNotSupported(Type wrapperType)
        {
            throw new ArgumentException(WrapperTypeNotSupportedFormat1.Format(wrapperType), nameof(wrapperType));
        }

        public static void ThrowIntOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, IntOutOfRangeCollection);
        }

        public static void ThrowPresenterCannotShowRequest(IReadOnlyMetadataContext request)
        {
            throw new ArgumentException(PresenterCannotShowRequestFormat1.Format(request.Dump()));
        }

        public static void ThrowPresenterInvalidRequest(IReadOnlyMetadataContext request, IReadOnlyMetadataContext response)
        {
            throw new ArgumentException(PresenterCannotHandleRequestFormat1.Format(request.Dump() + response.Dump()));
        }

        public static void ThrowNavigatingResultHasCallback()
        {
            throw new InvalidOperationException(NavigatingResultHasCallback);
        }

        public static void ThrowViewNotFound(Type viewModelType, Type? viewType = null)
        {
            var viewName = viewType == null ? "view" : viewType.FullName;
            throw new InvalidOperationException(ViewNotFoundFormat2.Format(viewName, viewModelType));
        }

        public static void ThrowCannotFindConstructor(Type service)
        {
            throw new InvalidOperationException(CannotFindConstructorFormat1.Format(service));
        }

        public static void ThrowIocCannotFindBinding(Type service)
        {
            throw new InvalidOperationException(IocCannotFindBindingFormat1.Format(service));
        }

        public static void ThrowIocCyclicalDependency(Type service)
        {
            throw new InvalidOperationException(IocCyclicalDependencyFormat1.Format(service));
        }

        public static void ThrowIocMoreThatOneBinding(Type service)
        {
            throw new InvalidOperationException(IocMoreThatOneBindingFormat1.Format(service));
        }

        public static void ThrowCannotGetViewModel(IReadOnlyMetadataContext metadata)
        {
            throw new InvalidOperationException(CannotGetViewModelFormat1.Format(metadata.Dump()));
        }

        public static void ThrowCannotGetComponent(object owner, Type componentType)
        {
            throw new InvalidOperationException(CannotGetComponentFormat2.Format(owner.GetType(), componentType));
        }

        public static void ThrowObjectDisposed(object item)
        {
            throw new ObjectDisposedException(item.GetType().FullName, ObjectDisposedFormat1.Format(item.GetType()));
        }

        public static void ThrowObjectNotInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(ObjectNotInitializedFormat2.Format((obj as Type ?? obj.GetType()).Name, hint));
        }

        public static void ThrowObjectInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(ObjectInitializedFormat2.Format(obj, hint));
        }

        public static void ThrowNotSupported(string msg)
        {
            throw new NotSupportedException(msg);
        }

        public static void ThrowDuplicateKey()
        {
            throw new ArgumentException(DuplicateKeyException);
        }

        public static void ThrowKeyNotFound()
        {
            throw new KeyNotFoundException();
        }

        #endregion
    }
}