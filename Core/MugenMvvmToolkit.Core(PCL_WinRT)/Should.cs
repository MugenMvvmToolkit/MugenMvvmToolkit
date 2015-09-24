#region Copyright

// ****************************************************************************
// <copyright file="Should.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
    /// <summary>
    ///     A static helper class that includes various parameter checking routines.
    /// </summary>
    public static class Should
    {
        #region Methods

        /// <summary>
        ///     Throws <see cref="ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <exception cref="ArgumentNullException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object argumentValue, [InvokerParameterName] string paramName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if the given argument is null or empty.
        /// </summary>
        /// <exception cref="ArgumentException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue))
                throw new ArgumentException(string.Format("Argument '{0}' cannot be null or empty", paramName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if the given argument is null or whitespace.
        /// </summary>
        /// <exception cref="ArgumentException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrWhitespace([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string argumentValue, [InvokerParameterName] string paramName)
        {
            if (string.IsNullOrEmpty(argumentValue) || (string.CompareOrdinal(argumentValue.Trim(), string.Empty) == 0))
                throw new ArgumentException(string.Format("Argument '{0}' cannot be null or whitespace", paramName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if the given argument is empty.
        /// </summary>
        /// <exception cref="ArgumentException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrEmpty<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T argumentValue, [InvokerParameterName] string paramName)
            where T : IEnumerable
        {
            if (argumentValue.IsNullOrEmpty())
                throw new ArgumentException(string.Format("Argument '{0}' cannot be null or empty", paramName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if the given argument is default.
        /// </summary>
        /// <exception cref="ArgumentException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotBeNullOrDefault<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T? argumentValue, [InvokerParameterName] string paramName)
            where T : struct
        {
            if (argumentValue == null || EqualityComparer<T>.Default.Equals(default(T), argumentValue.Value))
                throw new ArgumentException(string.Format("Argument '{0}' cannot be null or defult", paramName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException" /> if the given argument is default.
        /// </summary>
        /// <exception cref="ArgumentException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="paramName">Name of the parameter being tested. </param>
        [DebuggerStepThrough]
        public static void NotBeDefault<T>(T argumentValue, [InvokerParameterName] string paramName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(default(T), argumentValue))
                throw new ArgumentException(string.Format("Argument '{0}' cannot be default", paramName));
        }

        /// <summary>
        ///     Checks whether the specified <paramref name="instance" /> is of the specified <paramref name="requiredType" />.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="instance">The instance to check.</param>
        /// <param name="requiredType">The type to check for.</param>
        [DebuggerStepThrough]
        public static void BeOfType(object instance, string paramName, Type requiredType)
        {
            NotBeNull(instance, "instance");
            BeOfType(instance.GetType(), paramName, requiredType);
        }

        /// <summary>
        ///     Checks whether the specified <paramref name="instance" /> is of the specified T.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="instance">The instance to check.</param>
        [DebuggerStepThrough]
        public static void BeOfType<T>(object instance, string paramName)
        {
            BeOfType(instance, paramName, typeof(T));
        }

        /// <summary>
        ///     Checks whether the specified <paramref name="type" /> is of the specified <paramref name="requiredType" />.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="type">The type to check.</param>
        /// <param name="requiredType">The type to check for.</param>
        [DebuggerStepThrough]
        public static void BeOfType(Type type, string paramName, Type requiredType)
        {
            NotBeNull(type, "type");
            NotBeNull(requiredType, "requiredType");
            if (!requiredType.IsAssignableFrom(type))
                throw new ArgumentException(
                string.Format("Type '{0}' should be of type '{1}', but is not", type.Name, requiredType.Name), paramName);
        }

        /// <summary>
        ///     Checks whether the specified T is of the specified <paramref name="type" />.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="type">The type to check for.</param>
        [DebuggerStepThrough]
        public static void BeOfType<T>(Type type, string paramName)
        {
            BeOfType(type, paramName, typeof(T));
        }

        /// <summary>
        ///     Checks whether the passed in boolean check is <c>true</c>. If not, this method will throw a
        ///     <see cref="NotSupportedException" />.
        /// </summary>
        /// <param name="isSupported">if set to <c>true</c>, the action is supported; otherwise <c>false</c>.</param>
        /// <param name="error">The error message.</param>
        [DebuggerStepThrough, AssertionMethod]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string error)
        {
            if (!isSupported)
                throw new NotSupportedException(error);
        }

        /// <summary>
        ///     Checks whether the passed in boolean check is <c>true</c>. If not, this method will throw a
        ///     <see cref="NotSupportedException" />.
        /// </summary>
        /// <param name="isSupported">if set to <c>true</c>, the action is supported; otherwise <c>false</c>.</param>
        /// <param name="errorFormat">The error format.</param>
        /// <param name="args">The arguments for the string format.</param>
        [StringFormatMethod("errorFormat")]
        [DebuggerStepThrough, AssertionMethod]
        public static void BeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string errorFormat, params object[] args)
        {
            if (!isSupported)
                throw new NotSupportedException(string.Format(errorFormat, args));
        }

        /// <summary>
        ///     Checks whether the passed in boolean check is <c>true</c>. If not, this method will throw a
        ///     <see cref="NotSupportedException" />.
        /// </summary>
        /// <param name="isSupported">if set to <c>true</c>, the action is supported; otherwise <c>false</c>.</param>
        /// <param name="methodName">The specified method signature.</param>
        [DebuggerStepThrough, AssertionMethod]
        public static void MethodBeSupported([AssertionCondition(AssertionConditionType.IS_TRUE)]bool isSupported, string methodName)
        {
            BeSupported(isSupported, "The method " + methodName + " has not been implemented by this class.");
        }

        /// <summary>
        ///     Determines whether the specified argument is valid.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="validation">The validation function.</param>
        [DebuggerStepThrough]
        public static void BeValid(string paramName, [NotNull] Func<bool> validation)
        {
            NotBeNull(validation, "validation");
            BeValid(paramName, validation());
        }

        /// <summary>
        ///     Determines whether the specified argument is valid.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="validation">The validation function.</param>
        [DebuggerStepThrough]
        public static void BeValid<T>(T paramValue, string paramName, [NotNull] Func<T, bool> validation)
        {
            NotBeNull(validation, "validation");
            BeValid(paramName, validation(paramValue));
        }

        /// <summary>
        ///     Determines whether the specified argument is valid.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="validation">The validation function.</param>
        [DebuggerStepThrough, AssertionMethod]
        public static void BeValid(string paramName, [AssertionCondition(AssertionConditionType.IS_TRUE)] bool validation)
        {
            if (!validation)
                throw new ArgumentException(string.Format("Argument '{0}' is not valid", paramName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <exception cref="ArgumentNullException"> if tested value if null.</exception>
        /// <param name="value">Argument value to test.</param>
        /// <param name="propertyName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void PropertyNotBeNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]object value, [CallerMemberName] string propertyName = "")
        {
            if (value == null)
                throw new ArgumentNullException(propertyName,
                    string.Format("The property with name '{0}' cannot be null.", propertyName));
        }

        /// <summary>
        ///     Throws <see cref="ArgumentNullException" /> if the given argument is null or empty.
        /// </summary>
        /// <exception cref="ArgumentNullException"> if tested value if null.</exception>
        /// <param name="value">Argument value to test.</param>
        /// <param name="propertyName">Name of the parameter being tested. </param>
        [DebuggerStepThrough, AssertionMethod]
        public static void PropertyNotBeNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(propertyName,
                    string.Format("The property with name '{0}' cannot be null or empty.", propertyName));
        }

        /// <summary>
        ///     Makes sure that the object is not disposed.
        /// </summary>
        [DebuggerStepThrough]
        public static void NotBeDisposed(this IDisposableObject disposableObject)
        {
            NotBeNull(disposableObject, "disposableObject");
            if (disposableObject.IsDisposed)
                throw ExceptionManager.ObjectDisposed(disposableObject.GetType());
        }

        #endregion
    }
}