#region Copyright
// ****************************************************************************
// <copyright file="IObserver.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the observer that allows to observe an instance of object.
    /// </summary>
    public interface IObserver : IDisposable
    {
        /// <summary>
        ///     Gets the path.
        /// </summary>
        [NotNull]
        IBindingPath Path { get; }

        /// <summary>
        ///     Gets the underlying source value.
        /// </summary>
        [CanBeNull]
        object Source { get; }

        /// <summary>
        ///     Gets or sets the value changed listener.
        /// </summary>
        IHandler<ValueChangedEventArgs> Listener { get; set; }

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        void Update();

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
        ///     Gets the actual source object.
        /// </summary>
        [CanBeNull]
        object GetActualSource(bool throwOnError);

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        [NotNull]
        IBindingPathMembers GetPathMembers(bool throwOnError);
    }
}