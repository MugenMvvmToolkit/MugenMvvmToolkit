﻿using System;
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
        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if DEBUG
        public static void NotBeNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] [NoEnumeration]
            T? argumentValue, [InvokerParameterName] string paramName) where T : class
#else
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] [NoEnumeration]
            object? argumentValue, [InvokerParameterName] string paramName)
#endif
        {
            if (argumentValue == null)
                ExceptionManager.ThrowNullArgument(paramName);
        }

#pragma warning disable CS8777
        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull]
            string? argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                ExceptionManager.ThrowNullOrEmptyArgument(paramName);
        }
#pragma warning restore

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool isSupported, string error)
        {
            if (!isSupported)
                ExceptionManager.ThrowNotSupported(error);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeValid([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool validation, string paramName)
        {
            if (!validation)
                ExceptionManager.ThrowNotValidArgument(paramName);
        }

        [DebuggerStepThrough]
        public static void BeOfType([NotNull] [NoEnumeration] object? instance, [NotNull] Type? requiredType, string paramName)
        {
            NotBeNull(instance, paramName);
            NotBeNull(requiredType, nameof(requiredType));
            if (!requiredType.IsInstanceOfType(instance))
                ExceptionManager.ThrowArgumentShouldBeOfType(paramName, instance.GetType(), requiredType);
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>([NotNull] [NoEnumeration] object? instance, string paramName)
        {
            NotBeNull(instance, paramName);
            if (instance is not T)
                ExceptionManager.ThrowArgumentShouldBeOfType(paramName, instance.GetType(), typeof(T));
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>([NotNull] Type? type, string paramName) => BeOfType(type, typeof(T), paramName);

        [DebuggerStepThrough]
        public static void BeOfType([NotNull] Type? type, [NotNull] Type? requiredType, string paramName)
        {
            NotBeNull(type, nameof(type));
            NotBeNull(requiredType, nameof(requiredType));
            if (!requiredType.IsAssignableFrom(type))
                ExceptionManager.ThrowArgumentShouldBeOfType(paramName, type, requiredType);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool isSupported, string methodName) =>
            BeSupported(isSupported, MessageConstant.ShouldMethodBeSupportedFormat1.Format(methodName));
    }
}