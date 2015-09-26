#region Copyright

// ****************************************************************************
// <copyright file="IViewModel.cs">
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
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IViewModel : IDisposableObject, IObservable, ISuspendNotifications, IEventPublisher
    {
        bool IsInitialized { get; }

        bool IsBusy { get; }

        [CanBeNull]
        object BusyMessage { get; }

        [NotNull]
        IViewModelSettings Settings { get; }

        IIocContainer IocContainer { get; }

        CancellationToken DisposeCancellationToken { get; }

        void InitializeViewModel([NotNull] IDataContext context);

        [NotNull]
        IBusyToken BeginBusy(object message = null);

        [NotNull]
        IList<IBusyToken> GetBusyTokens();

        event EventHandler<IViewModel, EventArgs> Initialized;
    }
}
