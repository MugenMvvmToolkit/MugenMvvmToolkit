#region Copyright

// ****************************************************************************
// <copyright file="IReflectionManager.cs">
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
using System.Reflection;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the reflection access provider.
    /// </summary>
    public interface IReflectionManager
    {
        /// <summary>
        ///    Tries to creates a delegate of the specified type that represents the specified static or instance method, with the specified first argument.
        /// </summary>
        /// <returns>
        ///     A delegate of the specified type that represents the specified static method of the specified class.
        /// </returns>
        /// <param name="delegateType">The <see cref="T:System.Type" /> of delegate to create. </param>
        /// <param name="target">
        ///     The <see cref="T:System.Type" /> representing the class that implements <paramref name="method" />
        ///     .
        /// </param>
        /// <param name="method">The name of the static method that the delegate is to represent. </param>
        [CanBeNull]
        Delegate TryCreateDelegate([NotNull]Type delegateType, [CanBeNull] object target, [NotNull]MethodInfo method);

        /// <summary>
        ///     Gets a delegate to create an object using a <see cref="ConstructorInfo" />.
        /// </summary>
        /// <param name="constructor">
        ///     The specified <see cref="ConstructorInfo" />.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Func{TParams,TResult}" />
        /// </returns>
        [NotNull]
        Func<object[], object> GetActivatorDelegate([NotNull]ConstructorInfo constructor);

        /// <summary>
        ///     Gets a delegate to call the specified <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="method">
        ///     The specified <see cref="MethodInfo" />
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Func{TOwner,TParams,TResult}" />
        /// </returns>
        [NotNull]
        Func<object, object[], object> GetMethodDelegate([NotNull]MethodInfo method);

        /// <summary>
        ///     Gets a delegate to call the specified <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="delegateType">The type of delegate.</param>
        /// <param name="method">
        ///     The specified <see cref="MethodInfo" />
        /// </param>
        /// <returns>
        ///     An instance of delegate.
        /// </returns>
        [NotNull]
        Delegate GetMethodDelegate([NotNull]Type delegateType, [NotNull] MethodInfo method);

        /// <summary>
        ///     Gets a delegate to get a value in the specified <see cref="MemberInfo" />
        /// </summary>
        /// <typeparam name="TType">Type of the value.</typeparam>
        /// <param name="member">
        ///     The specified <see cref="MemberInfo" />.
        /// </param>
        [NotNull]
        Func<object, TType> GetMemberGetter<TType>([NotNull]MemberInfo member);

        /// <summary>
        ///     Gets a delegate to set specified value in the specified <see cref="MemberInfo" /> in a value type target, can be
        ///     used with reference type.
        /// </summary>
        /// <typeparam name="TType">Type of the value.</typeparam>
        /// <param name="member">
        ///     The specified <see cref="MemberInfo" />.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Action{TOwner,TType}" />
        /// </returns>
        [NotNull]
        Action<object, TType> GetMemberSetter<TType>([NotNull]MemberInfo member);
    }
}