using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm
{
    public static class Should
    {
        #region Methods

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object? argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string? argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentException(MessageConstants.ArgumentCannotBeNull.Format(paramName), paramName);
        }

        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool isSupported, string error)
        {
            if (!isSupported)
                ExceptionManager.ThrowNotSupported(error);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool validation)
        {
            if (!validation)
                throw new ArgumentException(MessageConstants.ArgumentNotValid.Format(paramName));
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
                throw new ArgumentException(MessageConstants.ArgumentShouldBeOfType.Format(type.Name, requiredType.Name), paramName);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool isSupported, string methodName)
        {
            BeSupported(isSupported, MessageConstants.ShouldMethodBeSupportedFormat1.Format(methodName));
        }

        [DebuggerStepThrough]
        public static void NotBeDisposed(this IViewModelBase viewModel)
        {
            if (viewModel.IsDisposed())
                ExceptionManager.ThrowObjectDisposed(viewModel.GetType());
        }

        #endregion
    }
}