#region Copyright

// ****************************************************************************
// <copyright file="ISuspendNotifications.cs">
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
using System.ComponentModel;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents an interface for suspend notifications.
    /// </summary>
    public interface ISuspendNotifications : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets a value indicating whether change notifications are suspended. <c>True</c> if notifications are suspended,
        ///     otherwise, <c>false</c>.
        /// </summary>
        bool IsNotificationsSuspended { get; }

        /// <summary>
        ///     Suspends the change notifications until the returned <see cref="IDisposable" /> is disposed.
        /// </summary>
        /// <returns>An instance of token.</returns>
        [NotNull]
        IDisposable SuspendNotifications();
    }
}