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

        int IComparer<T>.Compare(T x, T y)
        {
            return _compareDelegate(x, y);
        }

        #endregion
    }
}