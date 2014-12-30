#region Copyright

// ****************************************************************************
// <copyright file="ISubscriber.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the event subscriber.
    /// </summary>
    public interface ISubscriber : IEquatable<ISubscriber>
    {
        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ISubscriber" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ISubscriber" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        bool IsAlive { get; }

        /// <summary>
        ///     Gets a value indicating that <see cref="IEventAggregator"/> can add identical <see cref="ISubscriber"/> more than once.
        /// </summary>
        bool AllowDuplicate { get; }

        /// <summary>
        ///     Gets the target, if any.
        /// </summary>
        object Target { get; }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        /// <returns>The result of operation.</returns>
        HandlerResult Handle(object sender, object message);
    }
}