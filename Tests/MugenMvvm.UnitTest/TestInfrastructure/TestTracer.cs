using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces;

namespace MugenMvvm.UnitTest.TestInfrastructure
{
    public class TestTracer : ITracer
    {
        #region Properties

        public Func<TraceLevel, bool>? CanTrace { get; set; }

        public Action<TraceLevel, string>? Trace { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ITracer.CanTrace(TraceLevel level)
        {
            return CanTrace?.Invoke(level) ?? false;
        }

        void ITracer.Trace(TraceLevel level, string message)
        {
            Trace?.Invoke(level, message);
        }

        #endregion
    }
}