#region Copyright

// ****************************************************************************
// <copyright file="SelectedItemChangedEventArgs.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

namespace MugenMvvmToolkit.Models.EventArg
{
    public class SelectedItemChangedEventArgs : ValueChangedEventArgs<object>
    {
        #region Constructors

        public SelectedItemChangedEventArgs(object oldValue, object newValue)
            : base(oldValue, newValue)
        {
        }

        #endregion
    }

    public class SelectedItemChangedEventArgs<T> : SelectedItemChangedEventArgs
    {
        #region Constructors

        public SelectedItemChangedEventArgs(T oldValue, T newValue)
            : base(oldValue, newValue)
        {
        }

        #endregion

        #region Properties

        public new T OldValue => (T) base.OldValue;

        public new T NewValue => (T) base.NewValue;

        #endregion
    }
}
