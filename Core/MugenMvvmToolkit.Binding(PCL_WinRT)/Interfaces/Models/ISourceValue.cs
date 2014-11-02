#region Copyright
// ****************************************************************************
// <copyright file="ISourceValue.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface of source value wrapper.
    /// </summary>
    public interface ISourceValue
    {
        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ISourceValue" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ISourceValue" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        bool IsAlive { get; }

        /// <summary>
        ///     Gets the current source value.
        /// </summary>
        [CanBeNull]
        object Value { get; }

        /// <summary>
        ///     Occurs when the <see cref="Value"/>  property changed.
        /// </summary>
        event EventHandler<ISourceValue, EventArgs> ValueChanged;
    }
}