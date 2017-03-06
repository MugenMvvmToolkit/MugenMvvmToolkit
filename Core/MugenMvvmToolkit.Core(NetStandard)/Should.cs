#region Copyright

// ****************************************************************************
// <copyright file="Should.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit
{
    public static class Should
    {
        #region Methods

        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentException($"Argument '{paramName}' cannot be null or empty");
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrWhitespace([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrWhiteSpace(argumentValue))
                throw new ArgumentException($"Argument '{paramName}' cannot be null or whitespace");
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrEmpty<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T argumentValue, [InvokerParameterName] string paramName)
            where T : IEnumerable
        {
            if (argumentValue.IsNullOrEmpty())
                throw new ArgumentException($"Argument '{paramName}' cannot be null or empty");
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrDefault<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T? argumentValue, [InvokerParameterName] string paramName)
            where T : struct
        {
            if (argumentValue == null || EqualityComparer<T>.Default.Equals(default(T), argumentValue.Value))
                throw new ArgumentException($"Argument '{paramName}' cannot be null or defult");
        }

        [DebuggerStepThrough]
        public static void NotBeDefault<T>(T argumentValue, [InvokerParameterName] string paramName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(default(T), argumentValue))
                throw new ArgumentException($"Argument '{paramName}' cannot be default");
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
            if (!requiredType.IsAssignableFrom(type))
                throw new ArgumentException($"Type '{type.Name}' should be of type '{requiredType.Name}', but is not", paramName);
        }

        [DebuggerStepThrough]
        public static void BeOfType<T>(Type type, string paramName)
        {
            BeOfType(type, paramName, typeof(T));
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string error)
        {
            if (!isSupported)
                throw new NotSupportedException(error);
        }

        [StringFormatMethod("errorFormat")]
        [DebuggerStepThrough, AssertionMethod]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string errorFormat, params object[] args)
        {
            if (!isSupported)
                throw new NotSupportedException(string.Format(errorFormat, args));
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string methodName)
        {
            BeSupported(isSupported, "The method " + methodName + " has not been implemented by this class.");
        }

        [DebuggerStepThrough]
        public static void BeValid(string paramName, [NotNull] Func<bool> validation)
        {
            NotBeNull(validation, nameof(validation));
            BeValid(paramName, validation());
        }

        [DebuggerStepThrough]
        public static void BeValid<T>(T paramValue, string paramName, [NotNull] Func<T, bool> validation)
        {
            NotBeNull(validation, nameof(validation));
            BeValid(paramName, validation(paramValue));
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)] bool validation)
        {
            if (!validation)
                throw new ArgumentException($"Argument '{paramName}' is not valid");
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void PropertyNotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object value, [CallerMemberName] string propertyName = "")
        {
            if (value == null)
                throw new ArgumentNullException(propertyName, $"The property with name '{propertyName}' cannot be null.");
        }

        [DebuggerStepThrough, AssertionMethod]
        public static void PropertyNotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(propertyName, $"The property with name '{propertyName}' cannot be null or empty.");
        }

        [DebuggerStepThrough]
        public static void NotBeDisposed(this IDisposableObject disposableObject)
        {
            NotBeNull(disposableObject, nameof(disposableObject));
            if (disposableObject.IsDisposed)
                throw ExceptionManager.ObjectDisposed(disposableObject.GetType());
        }

        #endregion
    }
}
