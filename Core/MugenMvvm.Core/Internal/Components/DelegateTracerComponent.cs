using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public sealed class DelegateTracerComponent : ITracerComponent
    {
        #region Fields

        private readonly Func<TraceLevel, IReadOnlyMetadataContext?, bool> _canTrace;
        private readonly Action<TraceLevel, string, Exception?, IReadOnlyMetadataContext?> _trace;

        #endregion

        #region Constructors

        public DelegateTracerComponent(Action<TraceLevel, string, Exception?, IReadOnlyMetadataContext?> trace, Func<TraceLevel, IReadOnlyMetadataContext?, bool> canTrace)
        {
            Should.NotBeNull(trace, nameof(trace));
            Should.NotBeNull(canTrace, nameof(canTrace));
            _trace = trace;
            _canTrace = canTrace;
        }

        #endregion

        #region Implementation of interfaces

        public bool CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata)
        {
            return _canTrace(level, metadata);
        }

        public void Trace(TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            if (_canTrace(level, metadata))
                _trace(level, message, exception, metadata);
        }

        #endregion
    }
}