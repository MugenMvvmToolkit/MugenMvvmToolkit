#region Copyright

// ****************************************************************************
// <copyright file="NotifyCollectionChangingEventArgs.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        #region Fields

        private readonly NotifyCollectionChangedEventArgs _changedEventArgs;

        #endregion

        #region Constructors

        public NotifyCollectionChangingEventArgs([NotNull] NotifyCollectionChangedEventArgs changedEventArgs)
        {
            Should.NotBeNull(changedEventArgs, nameof(changedEventArgs));
            _changedEventArgs = changedEventArgs;
        }

        #endregion

        #region Properties

        [NotNull]
        public NotifyCollectionChangedEventArgs ChangedEventArgs => _changedEventArgs;

        public NotifyCollectionChangedAction Action => ChangedEventArgs.Action;

        public IList NewItems => ChangedEventArgs.NewItems;

        public IList OldItems => ChangedEventArgs.OldItems;

        public int NewStartingIndex => ChangedEventArgs.NewStartingIndex;

        public int OldStartingIndex => ChangedEventArgs.OldStartingIndex;

        #endregion
    }
}
