using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using static MugenMvvm.Constants.MessageConstant;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        #region Methods

        [DoesNotReturn]
        public static void ThrowAmbiguousMappingMatchFound()
        {
            throw new InvalidOperationException(AmbiguousMappingMatchFound);
        }

        [DoesNotReturn]
        public static void ThrowNullArgument(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        [DoesNotReturn]
        public static void ThrowNullOrEmptyArgument(string paramName)
        {
            throw new ArgumentException(ArgumentCannotBeNull.Format(paramName), paramName);
        }

        [DoesNotReturn]
        public static void ThrowNotValidArgument(string paramName)
        {
            throw new ArgumentException(ArgumentNotValid.Format(paramName));
        }

        [DoesNotReturn]
        public static void ThrowArgumentShouldBeOfType(string paramName, Type type, Type requiredType)
        {
            throw new ArgumentException(ArgumentShouldBeOfType.Format(type.Name, requiredType.Name), paramName);
        }

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
        public static void ThrowPresenterCannotShowRequest<TRequest>(TRequest request, IReadOnlyMetadataContext? metadata)
        {
            throw new InvalidOperationException(PresenterCannotShowRequestFormat2.Format(request, metadata.Dump()));
        }

        [DoesNotReturn]
        public static void ThrowCannotResolveService(object service)
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
        public static void ThrowRequestNotSupported<T>(object obj, T[]? _, object? request, IReadOnlyMetadataContext? metadata)
        {
            ThrowRequestNotSupported<T>(obj, request, metadata);
        }

        [DoesNotReturn]
        public static void ThrowRequestNotSupported<T>(object obj, object? request, IReadOnlyMetadataContext? metadata)
        {
            throw new InvalidOperationException(ObjectNotInitializedOrRequestNotSupportedFormat4.Format(obj, typeof(T).Name, request, metadata.Dump()));
        }

        [DoesNotReturn]
        public static void ThrowObjectNotInitialized(object obj, [CallerMemberName] string? hint = null)
        {
            throw new InvalidOperationException(ObjectNotInitializedFormat2.Format((obj as Type ?? obj.GetType()).Name, hint));
        }

        [DoesNotReturn]
        public static void ThrowObjectInitialized(object obj, [CallerMemberName] string? hint = null)
        {
            throw new InvalidOperationException(ObjectInitializedFormat2.Format(obj, hint));
        }

        [DoesNotReturn]
        public static void ThrowNotSupported(string msg)
        {
            throw new NotSupportedException(msg);
        }

        #endregion
    }
}