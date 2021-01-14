using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using static MugenMvvm.Constants.MessageConstant;
using static MugenMvvm.Bindings.Constants.BindingMessageConstant;

namespace MugenMvvm
{
    internal static class ExceptionManager
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNullArgument(string paramName) => throw new ArgumentNullException(paramName);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNullOrEmptyArgument(string paramName) => throw new ArgumentException(ArgumentCannotBeNull.Format(paramName), paramName);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotValidArgument(string paramName) => throw new ArgumentException(ArgumentNotValid.Format(paramName));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentShouldBeOfType(string paramName, Type type, Type requiredType) =>
            throw new ArgumentException(ArgumentShouldBeOfType.Format(type.Name, requiredType.Name), paramName);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowEnumIsNotValid<T>(T value) => throw new ArgumentException(EnumIsNotValidFormat2.Format(value!.ToString(), value!.GetType()), nameof(value));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCapacityLessThanCollection(string paramName) => throw new ArgumentOutOfRangeException(paramName, CapacityShouldBeGreaterOrEqual);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowIndexOutOfRangeCollection(string paramName) => throw new ArgumentOutOfRangeException(paramName, IndexMustBeWithinBounds);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowEnumOutOfRange<T>(string paramName, T @enum) => throw new ArgumentOutOfRangeException(paramName, UnhandledEnumFormat1.Format(@enum!.ToString()));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCommandCannotBeExecuted() => throw new InvalidOperationException(CommandCannotBeExecutedString);

        [DoesNotReturn]
        public static void ThrowWrapperTypeNotSupported(Type wrapperType) => throw new ArgumentException(WrapperTypeNotSupportedFormat1.Format(wrapperType), nameof(wrapperType));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowPresenterCannotShowRequest<TRequest>(TRequest request, IReadOnlyMetadataContext? metadata) =>
            throw new InvalidOperationException(PresenterCannotShowRequestFormat2.Format(request, metadata.Dump()));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotResolveService(object service) => throw new InvalidOperationException(CannotResolveService.Format(service));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotGetComponent(object owner, Type componentType) =>
            throw new InvalidOperationException(CannotGetComponentFormat2.Format(owner.GetType(), componentType));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectDisposed(object item) => throw new ObjectDisposedException(item.GetType().FullName, ObjectDisposedFormat1.Format(item.GetType()));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowRequestNotSupported<T>(object obj, object? request, IReadOnlyMetadataContext? metadata) =>
            throw new InvalidOperationException(ObjectNotInitializedOrRequestNotSupportedFormat4.Format(obj, typeof(T).Name, request, metadata.Dump("")));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectNotInitialized(object obj, [CallerMemberName] string? hint = null) =>
            throw new InvalidOperationException(ObjectNotInitializedFormat2.Format((obj as Type ?? obj.GetType()).Name, hint));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectInitialized(object obj, [CallerMemberName] string? hint = null) =>
            throw new InvalidOperationException(ObjectInitializedFormat2.Format(obj, hint));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupported(string msg) => throw new NotSupportedException(msg);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowMultiplePresenterResultNotSupported() => throw new NotSupportedException(MultiplePresenterResultNotSupported);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowMementoRequiredContextKey() => throw new NotSupportedException(ShouldSetContextKeyMemento);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCanceledException() => throw new OperationCanceledException();

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotAddComponent(IComponentCollection collection, object component) =>
            throw new InvalidOperationException(CannotAddComponentFormat2.Format(collection.Owner, component));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowAmbiguousMatchFound() => throw new AmbiguousMatchException();

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowBindingMemberMustBeWritable(IMemberInfo member) =>
            throw new InvalidOperationException(BindingMemberMustBeWritableFormat4.Format(member.Name, member.Type, member.MemberType, member.UnderlyingMember));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowBindingMemberMustBeReadable(IMemberInfo member) =>
            throw new InvalidOperationException(BindingMemberMustBeReadableFormat4.Format(member.Name, member.Type, member.MemberType, member.UnderlyingMember));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidBindingMember(Type sourceType, string path) => throw new InvalidOperationException(InvalidBindingMemberFormat2.Format(path, sourceType));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidBindingMember(object target, string path) => ThrowInvalidBindingMember(target as Type ?? target.GetType(), path);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotParseExpression(object? expression, string? hint = null) =>
            throw new InvalidOperationException(CannotParseExpressionFormat2.Format(expression, hint));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotCompileExpression(IExpressionNode expression, string? hint = null) =>
            throw new InvalidOperationException(CannotCompileExpressionFormat2.Format(expression, hint));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotUseExpressionExpected(IExpressionNode expression, Type expectedType) =>
            throw new InvalidOperationException(CannotUseExpressionExpected.Format(expression, expectedType));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowExpressionNodeCannotBeNull(Type ownerType) => throw new InvalidOperationException(ExpressionNodeCannotBeNullFormat1.Format(ownerType));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotResolveType(string typeName) => throw new InvalidOperationException(CannotResolveTypeFormat1.Format(typeName));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotResolveResource(string resource) => throw new InvalidOperationException(CannotResolveResourceFormat1.Format(resource));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotParseBindingParameter(string parameterName, object expectedValue, object currentValue) =>
            throw new InvalidOperationException(CannotParseBindingParameterFormat3.Format(parameterName, expectedValue, currentValue));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotConvertType(object? value, Type type) => throw new InvalidOperationException(CannotConvertTypeFormat2.Format(value, type));

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCannotUseExpressionClosure(object expression) => throw new InvalidOperationException(CannotUseExpressionClosureFormat1.Format(expression));
    }
}