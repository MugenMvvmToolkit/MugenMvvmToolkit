#define DEBUG

using System.Diagnostics;
using MugenMvvm.Interfaces;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure
{
    public class Tracer : ITracer
    {
        #region Constructors

        static Tracer()
        {
            Singleton<ITracer>.Initialize(new Tracer());
            var isAttached = Debugger.IsAttached;
            TraceWarning = isAttached;
            TraceError = isAttached;
        }

        #endregion

        #region Properties

        public static bool TraceInformation { get; set; }

        public static bool TraceWarning { get; set; }

        public static bool TraceError { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                TraceInternal(level, message);
        }

        public bool CanTrace(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    return TraceInformation;
                case TraceLevel.Warning:
                    return TraceWarning;
                case TraceLevel.Error:
                    return TraceError;
                default:
                    return false;
            }
        }

        #endregion

        #region Methods

        protected virtual void TraceInternal(TraceLevel level, string message)
        {
            Debug.WriteLine(level + ": " + message);
        }

        #endregion
    }
}