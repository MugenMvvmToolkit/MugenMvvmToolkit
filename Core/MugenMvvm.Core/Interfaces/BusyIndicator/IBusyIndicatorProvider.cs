using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyIndicatorProvider : IHasListeners<IBusyIndicatorProviderListener>, ISuspendNotifications, IDisposable
    {
        IBusyInfo? BusyInfo { get; }

        IBusyToken Begin(IBusyToken parentToken, int millisecondsDelay = 0);

        IBusyToken Begin(object? message, int millisecondsDelay = 0);

        IReadOnlyList<IBusyToken> GetTokens();
    }
}