#region Copyright

// ****************************************************************************
// <copyright file="EndBusyMessage.cs">
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

namespace MugenMvvmToolkit.Models.Messages
{
    /// <summary>
    ///     Represents the remove busy operation message.
    /// </summary>
    [Serializable]
    public class EndBusyMessage
    {
        #region Fields

        private readonly Guid _id;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EndBusyMessage" /> class.
        /// </summary>
        public EndBusyMessage(Guid id)
        {
            _id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the id of operation.
        /// </summary>
        public Guid Id
        {
            get { return _id; }
        }

        #endregion
    }
}