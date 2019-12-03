using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Metadata;
using static MugenMvvm.Constants.MessageConstant;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        [DoesNotReturn]
        public static void ThrowEnumIsNotValid<T>(T value)
        {
            throw new ArgumentException(EnumIsNotValidFormat2.Format(value!.ToString(), value!.GetType()), nameof(value));
        }

        [DoesNotReturn]
        public static void ThrowCapacityLessThanCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, CapacityShouldBeGreaterOrEqual);
        }

        [DoesNotReturn]
        public static void ThrowIndexOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, IndexMustBeWithinBounds);
        }

        [DoesNotReturn]
        public static void ThrowEnumOutOfRange<T>(string paramName, T @enum)
        {
            throw new ArgumentOutOfRangeException(paramName, UnhandledEnumFormat1.Format(@enum!.ToString()));
        }

        [DoesNotReturn]
        public static void ThrowCommandCannotBeExecuted()
        {
            throw new InvalidOperationException(CommandCannotBeExecutedString);
        }

        [DoesNotReturn]
        public static void ThrowWrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            throw new ArgumentException(WrapperTypeShouldBeNonAbstractFormat1.Format(wrapperType), nameof(wrapperType));
        }

        [DoesNotReturn]
        public static void ThrowWrapperTypeNotSupported(Type wrapperType)
        {
            throw new ArgumentException(WrapperTypeNotSupportedFormat1.Format(wrapperType), nameof(wrapperType));
        }

        [DoesNotReturn]
        public static void ThrowIntOutOfRangeCollection(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, IntOutOfRangeCollection);
        }

        [DoesNotReturn]
        public static void ThrowPresenterCannotShowRequest(IReadOnlyMetadataContext request)
        {
            throw new ArgumentException(PresenterCannotShowRequestFormat1.Format(request.Dump()));
        }

        [DoesNotReturn]
        public static void ThrowCannotFindConstructor(Type service)
        {
            throw new InvalidOperationException(CannotFindConstructorFormat1.Format(service));
        }

        [DoesNotReturn]
        public static void ThrowCannotResolveService(Type service)
        {
            throw new InvalidOperationException(CannotResolveService.Format(service));
        }

        [DoesNotReturn]
        public static void ThrowCannotGetComponent(object owner, Type componentType)
        {
            throw new InvalidOperationException(CannotGetComponentFormat2.Format(owner.GetType(), componentType));
        }

        [DoesNotReturn]
        public static void ThrowObjectDisposed(object item)
        {
            throw new ObjectDisposedException(item.GetType().FullName, ObjectDisposedFormat1.Format(item.GetType()));
        }

        [DoesNotReturn]
        public static void ThrowObjectNotInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(ObjectNotInitializedFormat2.Format((obj as Type ?? obj.GetType()).Name, hint));
        }

        [DoesNotReturn]
        public static void ThrowObjectInitialized(object obj, string? hint = null)
        {
            throw new InvalidOperationException(ObjectInitializedFormat2.Format(obj, hint));
        }

        [DoesNotReturn]
        public static void ThrowNotSupported(string msg)
        {
            throw new NotSupportedException(msg);
        }

        [DoesNotReturn]
        public static void ThrowDuplicateKey()
        {
            throw new ArgumentException(DuplicateKeyException);
        }

        [DoesNotReturn]
        public static void ThrowKeyNotFound()
        {
            throw new KeyNotFoundException();
        }

        [DoesNotReturn]
        public static void ThrowDecoratorComponentWithTheSamePriorityNotSupported(int priority, object currentComponent, object newComponent)
        {
            throw new NotSupportedException(DecoratorComponentWithTheSamePriorityNotSupportedFormat3.Format(priority, currentComponent, newComponent));
        }

        #endregion
    }
}