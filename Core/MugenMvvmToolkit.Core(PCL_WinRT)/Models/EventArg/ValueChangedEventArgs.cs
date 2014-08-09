#region Copyright
// ****************************************************************************
// <copyright file="ValueChangedEventArgs.cs">
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

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueChangedEventArgs{T}" /> class.
        /// </summary>
        protected ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the old value.
        /// </summary>
        [CanBeNull]
        public T OldValue { get; protected set; }

        /// <summary>
        ///     Gets the new value.
        /// </summary>
        [CanBeNull]
        public T NewValue { get; protected set; }

        #endregion
    }
}