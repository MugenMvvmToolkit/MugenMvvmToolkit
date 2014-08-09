#region Copyright
// ****************************************************************************
// <copyright file="IHasSelfWeakReference.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface that indicates that the instance has a self weak reference
    /// </summary>
    public interface IHasSelfWeakReference
    {
        /// <summary>
        ///     Gets the self <see cref="WeakReference" />.
        /// </summary>
        WeakReference SelfReference { get; }
    }
}