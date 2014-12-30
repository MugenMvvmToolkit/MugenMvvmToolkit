#region Copyright

// ****************************************************************************
// <copyright file="AsyncValidationMessage.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models.Messages
{
    /// <summary>
    ///     Provides data for the error changed event.
    /// </summary>
    [Serializable]
    public class AsyncValidationMessage
    {
        #region Fields

        private readonly Guid _id;
        private readonly bool _isEndOperation;
        private readonly string _propertyName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncValidationMessage" /> class.
        /// </summary>
        public AsyncValidationMessage(Guid id, string propertyName, bool isEndOperation)
        {
            _id = id;
            _isEndOperation = isEndOperation;
            _propertyName = propertyName;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value that indicates that this operation is final.
        /// </summary>
        public bool IsEndOperation
        {
            get { return _isEndOperation; }
        }

        /// <summary>
        ///     Gets the name of property, if any.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        ///     Gets the id of operation.
        /// </summary>
        public Guid Id
        {
            get { return _id; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts current message to an instance of <c>AsyncValidationMessage</c>.
        /// </summary>
        /// <returns>An instance of <c>AsyncValidationMessage</c>.</returns>
        [NotNull]
        public AsyncValidationMessage ToEndMessage()
        {
            return new AsyncValidationMessage(Id, PropertyName, true);
        }

        #endregion
    }
}