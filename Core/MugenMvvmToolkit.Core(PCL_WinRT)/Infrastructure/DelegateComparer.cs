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
    public sealed class DelegateComparer<T> : IComparer<T>
    {
        #region Fields

        private readonly Comparison<T> _compareDelegate;

        #endregion

        #region Constructors

        public DelegateComparer(Comparison<T> comparerDelegate)
        {
            Should.NotBeNull(comparerDelegate, nameof(comparerDelegate));
            _compareDelegate = comparerDelegate;
        }

        #endregion

        #region Implementation of IComparer<T>

        public int Compare(T x, T y)
        {
            return _compareDelegate(x, y);
        }

        #endregion
    }
}
