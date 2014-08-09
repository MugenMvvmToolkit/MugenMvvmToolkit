#region Copyright
// ****************************************************************************
// <copyright file="IEventAggregator.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator : IHandler<object>, IObservable
    {
        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
        void Publish([NotNull] object sender, [NotNull] object message);

        /// <summary>
        ///     Gets the collection of observers.
        /// </summary>
        [NotNull]
        IList<object> GetObservers();

        /// <summary>
        ///     Removes all listeners.
        /// </summary>
        void Clear();
    }
}