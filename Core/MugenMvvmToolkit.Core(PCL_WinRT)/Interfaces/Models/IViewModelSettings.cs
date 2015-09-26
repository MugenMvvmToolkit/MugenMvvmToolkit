#region Copyright

// ****************************************************************************
// <copyright file="IViewModelSettings.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IViewModelSettings
    {
        bool BroadcastAllMessages { get; set; }

        bool DisposeIocContainer { get; set; }

        bool DisposeCommands { get; set; }

        HandleMode HandleBusyMessageMode { get; set; }

        object DefaultBusyMessage { get; set; }

        ExecutionMode EventExecutionMode { get; set; }

        [NotNull]
        IDataContext Metadata { get; }

        [NotNull]
        IDataContext State { get; }

        [NotNull]
        IViewModelSettings Clone();
    }
}
