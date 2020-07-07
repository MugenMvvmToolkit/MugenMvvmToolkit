using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;

namespace MugenMvvm
{
    public static class Should
    {
        #region Methods

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            [NotNull]object? argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                ExceptionManager.ThrowNullArgument(paramName);
        }

#pragma warning disable CS8777
        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            [NotNull]string? argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                ExceptionManager.ThrowNullOrEmptyArgument(paramName);
        }
#pragma warning restore

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool isSupported, string error)
        {
            if (!isSupported)
                ExceptionManager.ThrowNotSupported(error);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool validation)
        {
            if (!validation)
                ExceptionManager.ThrowNotValidArgument(paramName);
        }

        [DebuggerStepThrough]
        public static void BeOfType([NotNull]object? instance, string paramName, [NotNull]Type? requiredType)
        {
            NotBeNull(instance, paramName);
            BeOfType(instance.GetType(), paramName, requiredType);
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>([NotNull]object? instance, string paramName)
        {
            BeOfType(instance, paramName, typeof(T));
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>([NotNull]Type? type, string paramName)
        {
            BeOfType(type, paramName, typeof(T));
        }

        [DebuggerStepThrough]
        public static void BeOfType([NotNull]Type? type, string paramName, [NotNull]Type? requiredType)
        {
            NotBeNull(type, nameof(type));
            NotBeNull(requiredType, nameof(requiredType));
            if (!requiredType.IsAssignableFrom(type))
                ExceptionManager.ThrowArgumentShouldBeOfType(paramName, type, requiredType);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool isSupported, string methodName)
        {
            BeSupported(isSupported, MessageConstant.ShouldMethodBeSupportedFormat1.Format(methodName));
        }

        #endregion
    }
}