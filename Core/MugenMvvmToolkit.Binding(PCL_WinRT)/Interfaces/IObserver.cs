#region Copyright

// ****************************************************************************
// <copyright file="IObserver.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the observer that allows to observe an instance of object.
    /// </summary>
    public interface IObserver : IDisposable
    {
        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="IObserver" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="IObserver" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        bool IsAlive { get; }

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

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        event EventHandler<IObserver, ValueChangedEventArgs> ValueChanged;
    }
}