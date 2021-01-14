using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class DelegateLogger : ILoggerComponent, IHasPriority
    {
        private readonly Func<LogLevel, IReadOnlyMetadataContext?, bool> _canLog;
        private readonly Action<LogLevel, object, Exception?, IReadOnlyMetadataContext?> _log;

        public DelegateLogger(Action<LogLevel, object, Exception?, IReadOnlyMetadataContext?> log, Func<LogLevel, IReadOnlyMetadataContext?, bool> canLog)
        {
            Should.NotBeNull(log, nameof(log));
            Should.NotBeNull(canLog, nameof(canLog));
            _log = log;
            _canLog = canLog;
        }

        public int Priority { get; set; } = InternalComponentPriority.Logger;

        public bool CanLog(ILogger logger, LogLevel level, IReadOnlyMetadataContext? metadata) => _canLog(level, metadata);

        public void Log(ILogger logger, LogLevel level, object message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            if (_canLog(level, metadata))
                _log(level, message, exception, metadata);
        }
    }
}