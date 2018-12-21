using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.BusyIndicator
{
    public interface IBusyInfo
    {
        object? Message { get; }

        bool TryGetMessage<TType>(out TType message, Func<TType, bool>? filter = null);

        IReadOnlyList<object?> GetMessages();
    }
}