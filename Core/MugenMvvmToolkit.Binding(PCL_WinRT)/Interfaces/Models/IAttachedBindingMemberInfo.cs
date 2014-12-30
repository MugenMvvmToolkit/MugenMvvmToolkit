#region Copyright

// ****************************************************************************
// <copyright file="IAttachedBindingMemberInfo.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the attached binding member info.
    /// </summary>
    public interface IAttachedBindingMemberInfo<in TTarget, TType> : IBindingMemberInfo
    {
        /// <summary>
        ///     Returns the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be returned.</param>
        /// <param name="args">Optional values for members.</param>
        /// <returns>The member value of the specified object.</returns>
        TType GetValue(TTarget source, [CanBeNull] object[] args);

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="value">The value for member.</param>
        object SetValue(TTarget source, TType value);

        /// <summary>
        ///     Sets the member value of a specified object.
        /// </summary>
        /// <param name="source">The object whose member value will be set.</param>
        /// <param name="args">Optional values for members.</param>
        object SetValue(TTarget source, object[] args);

        /// <summary>
        ///     Attempts to track the value change.
        /// </summary>
        IDisposable TryObserve(TTarget source, IEventListener listener);
    }

    /// <summary>
    ///     Represents the notifiable attached binding member info.
    /// </summary>
    public interface INotifiableAttachedBindingMemberInfo<in TTarget, TType> : IAttachedBindingMemberInfo<TTarget, TType>
    {
        /// <summary>
        ///     Raises the member changed event.
        /// </summary>
        void Raise(TTarget target, object message);
    }
}