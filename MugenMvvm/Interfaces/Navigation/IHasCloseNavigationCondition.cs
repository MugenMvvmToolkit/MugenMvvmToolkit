﻿using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasCloseNavigationCondition
    {
        Task<bool> CanCloseAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}