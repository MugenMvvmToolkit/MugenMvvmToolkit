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
            object? argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                ExceptionManager.ThrowNullArgument(paramName);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            string? argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                ExceptionManager.ThrowNullOrEmptyArgument(paramName);
        }

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
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool validation)
        {
            if (!validation)
                ExceptionManager.ThrowNotValidArgument(paramName);
        }

        [DebuggerStepThrough]
        public static void BeOfType(object instance, string paramName, Type requiredType)
        {
            NotBeNull(instance, paramName);
            BeOfType(instance.GetType(), paramName, requiredType);
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>(object instance, string paramName)
        {
            BeOfType(instance, paramName, typeof(T));
        }

        [DebuggerStepThrough]
        public static void BeOfType(Type type, string paramName, Type requiredType)
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