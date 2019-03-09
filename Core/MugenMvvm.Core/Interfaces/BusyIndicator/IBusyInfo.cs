using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyInfo
    {
        IBusyToken Token { get; }

        IBusyToken? TryGetToken(Func<IBusyToken, bool> filter);

        IReadOnlyList<IBusyToken> GetTokens();
    }
}