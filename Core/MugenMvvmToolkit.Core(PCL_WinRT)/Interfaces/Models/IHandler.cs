#region Copyright
// ****************************************************************************
// <copyright file="IHandler.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the interface that allows to handle an event.
    /// </summary>
    public interface IHandler<in TMessage>
    {
        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void Handle([NotNull] object sender, [NotNull] TMessage message);
    }

    /// <summary>
    ///     Represents the message that can be sent through all the models in the chain.
    /// </summary>
    public interface IBroadcastMessage
    {
    }

    /// <summary>
    ///     Represents the message that should be traced.
    /// </summary>
    public interface ITracebleMessage
    {
    }
}