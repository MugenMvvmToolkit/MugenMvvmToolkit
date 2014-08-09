#region Copyright
// ****************************************************************************
// <copyright file="IBindingSource.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Sources
{
    /// <summary>
    ///     Represents the binding source.
    /// </summary>
    public interface IBindingSource : IDisposable
    {
        /// <summary>
        ///     Gets the path.
        /// </summary>
        [NotNull]
        IBindingPath Path { get; }

        /// <summary>
        ///     Determines whether the current source is valid.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid; false to return false.
        /// </param>
        /// <returns>
        ///     If <c>true</c> current source is valid, otherwise <c>false</c>.
        /// </returns>
        bool Validate(bool throwOnError);

        /// <summary>
        ///     Gets the source object.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid.
        /// </param>
        [CanBeNull]
        object GetSource(bool throwOnError);

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid.
        /// </param>
        [NotNull]
        IBindingPathMembers GetPathMembers(bool throwOnError);

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        event EventHandler<IBindingSource, ValueChangedEventArgs> ValueChanged;
    }
}