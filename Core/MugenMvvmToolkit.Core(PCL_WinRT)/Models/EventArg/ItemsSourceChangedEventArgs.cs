#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceChangedEventArgs.cs">
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
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ItemsSourceChangedEventArgs : EventArgs
    {
        #region Fields

        private readonly IEnumerable _value;

        #endregion

        #region Constructors

        public ItemsSourceChangedEventArgs([CanBeNull]IEnumerable value)
        {
            _value = value;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public IEnumerable Value => _value;

        #endregion
    }

    public class ItemsSourceChangedEventArgs<T> : ItemsSourceChangedEventArgs
    {
        #region Constructors

        public ItemsSourceChangedEventArgs([CanBeNull]IEnumerable<T> value)
            : base(value)
        {
        }

        #endregion

        #region Properties

        [CanBeNull]
        public new IEnumerable<T> Value => (IEnumerable<T>) base.Value;

        #endregion
    }
}
