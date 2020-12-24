using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILoggerComponent : IComponent<ILogger>
    {
        bool CanLog(ILogger logger, LogLevel level, IReadOnlyMetadataContext? metadata);

        void Log(ILogger logger, LogLevel level, object message, Exception? exception, IReadOnlyMetadataContext? metadata);
    }
}