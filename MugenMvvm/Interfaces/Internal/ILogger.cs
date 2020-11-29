using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILogger : IComponentOwner<ILogger>, IDisposable
    {
        ILogger? TryGetLogger(object request, IReadOnlyMetadataContext? metadata = null);

        bool CanLog(LogLevel level, IReadOnlyMetadataContext? metadata = null);

        void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null);
    }
}