#region Copyright
// ****************************************************************************
// <copyright file="StateChangedMessage.cs">
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
    ///     Represents the message that indicates that any state was changed.
    /// </summary>
    [Serializable]
    public class StateChangedMessage : IBroadcastMessage
    {
        #region Fields

        /// <summary>
        ///     Gets the empty message.
        /// </summary>
        public static readonly StateChangedMessage Empty = new StateChangedMessage();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateChangedMessage" /> class.
        /// </summary>
        protected StateChangedMessage()
        {
        }

        #endregion
    }
}