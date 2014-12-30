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

        /// <summary>
        ///     Initializes the <see cref="ItemsSourceChangedEventArgs" />.
        /// </summary>
        public ItemsSourceChangedEventArgs([CanBeNull]IEnumerable value)
        {
            _value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the new value of items source.
        /// </summary>
        [CanBeNull]
        public IEnumerable Value
        {
            get { return _value; }
        }

        #endregion
    }

    public class ItemsSourceChangedEventArgs<T> : ItemsSourceChangedEventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes the <see cref="ItemsSourceChangedEventArgs{T}" />.
        /// </summary>
        public ItemsSourceChangedEventArgs([CanBeNull]IEnumerable<T> value)
            : base(value)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the new value of items source.
        /// </summary>
        [CanBeNull]
        public new IEnumerable<T> Value
        {
            get { return (IEnumerable<T>) base.Value; }
        }

        #endregion
    }
}