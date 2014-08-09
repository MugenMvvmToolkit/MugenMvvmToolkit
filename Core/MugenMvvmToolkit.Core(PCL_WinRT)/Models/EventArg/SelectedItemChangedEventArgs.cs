#region Copyright
// ****************************************************************************
// <copyright file="SelectedItemChangedEventArgs.cs">
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
namespace MugenMvvmToolkit.Models.EventArg
{
    public class SelectedItemChangedEventArgs : ValueChangedEventArgs<object>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectedItemChangedEventArgs" /> class.
        /// </summary>
        public SelectedItemChangedEventArgs(object oldValue, object newValue)
            : base(oldValue, newValue)
        {
        }

        #endregion
    }

    public class SelectedItemChangedEventArgs<T> : SelectedItemChangedEventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectedItemChangedEventArgs" /> class.
        /// </summary>
        public SelectedItemChangedEventArgs(T oldValue, T newValue)
            : base(oldValue, newValue)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the old value.
        /// </summary>
        public new T OldValue
        {
            get { return (T) base.OldValue; }
        }

        /// <summary>
        ///     Gets the new value.
        /// </summary>
        public new T NewValue
        {
            get { return (T) base.NewValue; }
        }

        #endregion
    }
}