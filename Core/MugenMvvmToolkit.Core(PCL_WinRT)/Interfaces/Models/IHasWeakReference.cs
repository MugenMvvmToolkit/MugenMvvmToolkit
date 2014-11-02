#region Copyright
// ****************************************************************************
// <copyright file="IHasWeakReference.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface that indicates that the instance has a self weak reference
    /// </summary>
    public interface IHasWeakReference
    {
        /// <summary>
        ///     Gets the <see cref="System.WeakReference" /> of current object.
        /// </summary>
        [NotNull]
        WeakReference WeakReference { get; }
    }
}