#region Copyright
// ****************************************************************************
// <copyright file="DataErrorsChangedMessage.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models.Messages
{
    /// <summary>
    ///     Provides data for the error changed event.
    /// </summary>
    [Serializable]
    public class DataErrorsChangedMessage : IBroadcastMessage
    {
        #region Fields

        private readonly bool _isAsyncValidate;
        private readonly string _propertyName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataErrorsChangedMessage" /> class.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the property that has an error.  null or <see cref="F:System.String.Empty" /> if the error is
        ///     object-level.
        /// </param>
        /// <param name="isAsyncValidate">Indicates that property was async validation.</param>
        public DataErrorsChangedMessage(string propertyName, bool isAsyncValidate)
        {
            _propertyName = propertyName;
            _isAsyncValidate = isAsyncValidate;
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

        /// <summary>
        ///     Indicates that property was async validated.
        /// </summary>
        public bool IsAsyncValidate
        {
            get { return _isAsyncValidate; }
        }

        #endregion
    }
}