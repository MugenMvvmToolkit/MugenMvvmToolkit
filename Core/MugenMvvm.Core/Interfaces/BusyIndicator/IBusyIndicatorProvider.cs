using System.Collections.Generic;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyIndicatorProvider : ISuspendNotifications, IHasListeners<IBusyIndicatorProviderListener>
    {
        IBusyInfo? BusyInfo { get; }

        IBusyToken Begin(IBusyToken parentToken, int millisecondsDelay = 0);

        IBusyToken Begin(object? message, int millisecondsDelay = 0);

        IReadOnlyList<IBusyToken> GetTokens();
    }
}