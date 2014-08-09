#region Copyright
// ****************************************************************************
// <copyright file="IBindingContext.cs">
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
    ///     Represents the binding context.
    /// </summary>
    public interface IBindingContext
    {
        /// <summary>
        ///     Gets the source object.
        /// </summary>
        [CanBeNull]
        object Source { get; }

        /// <summary>
        ///     Gets or sets the data context.
        /// </summary>
        [CanBeNull]
        object DataContext { get; set; }

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<IBindingContext, EventArgs> DataContextChanged;
    }
}