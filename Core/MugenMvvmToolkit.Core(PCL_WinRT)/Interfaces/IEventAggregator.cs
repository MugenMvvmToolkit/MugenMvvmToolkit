#region Copyright

// ****************************************************************************
// <copyright file="IEventAggregator.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator : IObservable, IEventPublisher
    {
        /// <summary>
        ///     Determines whether the <see cref="IEventAggregator" /> contains a specific subscriber.
        /// </summary>
        bool Contains([NotNull] ISubscriber subscriber);

        /// <summary>
        ///     Removes all subscribers.
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        ///     Gets the collection of subscribers.
        /// </summary>
        [NotNull]
        IList<ISubscriber> GetSubscribers();
    }
}