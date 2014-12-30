#region Copyright

// ****************************************************************************
// <copyright file="BeginBusyMessage.cs">
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
    ///     Represents the busy operation message.
    /// </summary>
    [Serializable]
    public class BeginBusyMessage
    {
        #region Fields

        private readonly Guid _id;
        private readonly object _message;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginBusyMessage" /> class.
        /// </summary>
        public BeginBusyMessage(Guid id, object message)
        {
            _id = id;
            _message = message;
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

        /// <summary>
        ///     Gets the message
        /// </summary>
        [CanBeNull]
        public object Message
        {
            get { return _message; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts current message to an instance of <c>EndBusyMessage</c>.
        /// </summary>
        /// <returns>An instance of <c>EndBusyMessage</c>.</returns>
        [NotNull]
        public EndBusyMessage ToEndBusyMessage()
        {
            return new EndBusyMessage(Id);
        }

        #endregion
    }
}