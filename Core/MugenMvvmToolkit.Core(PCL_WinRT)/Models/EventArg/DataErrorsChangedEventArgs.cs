#region Copyright
// ****************************************************************************
// <copyright file="DataErrorsChangedEventArgs.cs">
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
#if NONOTIFYDATAERROR
using System;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.EventArg
{
    /// <summary>
    ///     Provides data for the <see cref="INotifyDataErrorInfo.ErrorsChanged" /> event.
    /// </summary>
    public class DataErrorsChangedEventArgs : EventArgs
    {
        #region Fields

        private readonly string _propertyName;

        #endregion
        
        #region Constructors

        /// <summary>
        /// Initializes the <see cref="DataErrorsChangedEventArgs"/>.
        /// </summary>
        public DataErrorsChangedEventArgs(string propertyName)
        {
            _propertyName = propertyName;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the name of the property that has an error.
        /// </summary>
        /// <returns>
        ///     The name of the property that has an error. null or <see cref="F:System.String.Empty" /> if the error is
        ///     object-level.
        /// </returns>
        public string PropertyName
        {
            get { return _propertyName; }            
        }

        #endregion
    }
}
#endif