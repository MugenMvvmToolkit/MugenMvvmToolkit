#region Copyright

// ****************************************************************************
// <copyright file="SynchronousThreadManager.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public sealed class SynchronousThreadManager : IThreadManager
    {
        #region Constructors

        public SynchronousThreadManager()
        {
            IsUiThread = true;
        }

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread { get; set; }

        public void InvokeOnUiThreadAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InvokeOnUiThread(action, priority, cancellationToken);
        }

        public void InvokeOnUiThread(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Should.NotBeNull(action, "action");
            if (!cancellationToken.IsCancellationRequested)
                action();
        }

        public void InvokeAsync(Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InvokeOnUiThread(action, priority, cancellationToken);
        }

        #endregion
    }
}
