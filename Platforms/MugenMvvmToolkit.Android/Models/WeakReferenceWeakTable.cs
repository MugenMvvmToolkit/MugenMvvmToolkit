#region Copyright

// ****************************************************************************
// <copyright file="WeakReferenceWeakTable.cs">
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
using System.Runtime.CompilerServices;

namespace MugenMvvmToolkit.Android.Models
{
    internal sealed class WeakReferenceWeakTable : WeakReference
    {
        #region Fields

        private readonly int _hash;

        #endregion

        #region Constructors

        public WeakReferenceWeakTable(object item)
            : base(item, true)
        {
            _hash = RuntimeHelpers.GetHashCode(item);
        }

        #endregion

        #region Overrides of WeakReference

        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion
    }
}