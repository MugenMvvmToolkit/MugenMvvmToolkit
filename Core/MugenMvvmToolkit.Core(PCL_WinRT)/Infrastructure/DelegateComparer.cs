#region Copyright

// ****************************************************************************
// <copyright file="DelegateComparer.cs">
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
using System.Collections.Generic;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the delegate comparer.
    /// </summary>
    public sealed class DelegateComparer<T> : IComparer<T>
    {
        #region Fields

        private readonly Comparison<T> _compareDelegate;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateComparer{T}" /> class.
        /// </summary>
        public DelegateComparer(Comparison<T> comparerDelegate)
        {
            Should.NotBeNull(comparerDelegate, "comparerDelegate");
            _compareDelegate = comparerDelegate;
        }

        #endregion

        #region Implementation of IComparer<T>

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
        ///     the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero
        ///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
        ///     <paramref name="y" />.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(T x, T y)
        {
            return _compareDelegate(x, y);
        }

        #endregion
    }
}