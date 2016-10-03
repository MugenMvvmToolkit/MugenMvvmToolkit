#region Copyright

// ****************************************************************************
// <copyright file="IThreadManager.cs">
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

using System;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IThreadManager
    {
        bool IsUiThread { get; }

        void InvokeOnUiThread([NotNull] Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken));

        void InvokeOnUiThreadAsync([NotNull] Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken));

        void InvokeAsync([NotNull] Action action, OperationPriority priority = OperationPriority.Normal,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
