#region Copyright

// ****************************************************************************
// <copyright file="IViewModel.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Requests;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Requests;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    public interface IViewModel : IDisposableObject, IObservable, ISuspendNotifications, IEventPublisher
    {
        bool IsInitialized { get; }

        bool IsBusy { get; }

        [CanBeNull]
        object BusyMessage { get; }

        [CanBeNull]
        IBusyInfo BusyInfo { get; }

        [NotNull]
        IViewModelSettings Settings { get; }

        IIocContainer IocContainer { get; }

        CancellationToken DisposeCancellationToken { get; }

        void InitializeViewModel([NotNull] IDataContext context);

        [NotNull]
        IBusyToken BeginBusy(object message = null);

        [NotNull]
        IList<IBusyToken> GetBusyTokens();

	    IRequestHandlerProvider RequestHandler { get; }

	    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) where TResponse : ResponseBase, new();

		event EventHandler<IViewModel, EventArgs> Initialized;
    }
}
