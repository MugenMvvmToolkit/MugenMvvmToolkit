#region Copyright
// ****************************************************************************
// <copyright file="NotifyCollectionChangingEventArgs.cs">
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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models.EventArg
{
    /// <summary>
    ///     Provides data for the CollectionChanging event.
    /// </summary>
    public class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        #region Fields

        private readonly NotifyCollectionChangedEventArgs _changedEventArgs;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyCollectionChangingEventArgs" /> class.
        /// </summary>
        public NotifyCollectionChangingEventArgs([NotNull] NotifyCollectionChangedEventArgs changedEventArgs)
        {
            Should.NotBeNull(changedEventArgs, "changedEventArgs");
            _changedEventArgs = changedEventArgs;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the underlying <see cref="NotifyCollectionChangedEventArgs" />.
        /// </summary>
        [NotNull]
        public NotifyCollectionChangedEventArgs ChangedEventArgs
        {
            get { return _changedEventArgs; }
        }

        /// <summary>
        ///     The action that caused the event.
        /// </summary>
        public NotifyCollectionChangedAction Action
        {
            get { return ChangedEventArgs.Action; }
        }

        /// <summary>
        ///     The items affected by the change.
        /// </summary>
        public IList NewItems
        {
            get { return ChangedEventArgs.NewItems; }
        }

        /// <summary>
        ///     The old items affected by the change (for Replace events).
        /// </summary>
        public IList OldItems
        {
            get { return ChangedEventArgs.OldItems; }
        }

        /// <summary>
        ///     The index where the change occurred.
        /// </summary>
        public int NewStartingIndex
        {
            get { return ChangedEventArgs.NewStartingIndex; }
        }

        /// <summary>
        ///     The old index where the change occurred (for Move events).
        /// </summary>
        public int OldStartingIndex
        {
            get { return ChangedEventArgs.OldStartingIndex; }
        }

        #endregion
    }
}