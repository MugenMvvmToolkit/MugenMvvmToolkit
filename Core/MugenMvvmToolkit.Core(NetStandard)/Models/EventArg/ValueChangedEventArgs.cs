#region Copyright

// ****************************************************************************
// <copyright file="ValueChangedEventArgs.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

        protected ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public T OldValue { get; protected set; }

        [CanBeNull]
        public T NewValue { get; protected set; }

        #endregion
    }
}
