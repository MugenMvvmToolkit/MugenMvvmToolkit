using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm
{
    public static class Should
    {
        #region Methods

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL), EnsuresNotNull] object? argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL), EnsuresNotNull] string? argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentException(MessageConstants.ArgumentCannotBeNull.Format(paramName), paramName);
        }

        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]
            bool isSupported, string error)
        {
            if (!isSupported)
                throw new NotSupportedException(error);
        }

        [DebuggerStepThrough]
        [AssertionMethod]
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)]
            bool validation)
        {
            if (!validation)
                throw new ArgumentException(MessageConstants.ArgumentIsNotValid.Format(paramName));
        }

        [DebuggerStepThrough]
        public static void BeOfType(object instance, string paramName, Type requiredType)
        {
            NotBeNull(instance, nameof(instance));
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
            if (!requiredType.IsAssignableFromUnified(type))
                throw new ArgumentException(MessageConstants.ArgumentShouldBeOfType.Format(type.Name, requiredType.Name), paramName);
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string methodName)
        {
            BeSupported(isSupported, MessageConstants.ShouldMethodBeSupportedFormat1.Format(methodName));
        }

        [DebuggerStepThrough]
        public static void NotBeDisposed(this IViewModel viewModel)
        {
            if (viewModel.IsDisposed())
                throw ExceptionManager.ObjectDisposed(viewModel.GetType());
        }

        #endregion
    }
}