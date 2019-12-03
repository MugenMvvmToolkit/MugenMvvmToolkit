using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class DelegateTracer : ITracer
    {
        #region Fields

        private readonly Func<TraceLevel, bool> _canTrace;
        private readonly Action<string> _trace;

        #endregion

        #region Constructors

        public DelegateTracer(Action<string> trace, Func<TraceLevel, bool> canTrace)
        {
            Should.NotBeNull(trace, nameof(trace));
            Should.NotBeNull(canTrace, nameof(canTrace));
            _trace = trace;
            _canTrace = canTrace;
        }

        #endregion

        #region Implementation of interfaces

        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                _trace(level + ": " + message);
        }

        public bool CanTrace(TraceLevel level)
        {
            return _canTrace(level);
        }

        #endregion
    }
}