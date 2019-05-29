using System;
using System.Diagnostics;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class Tracer : ITracer
    {
        #region Fields

        private readonly Action<string> _traceDelegate;

        #endregion

        #region Constructors

        public Tracer(Action<string> traceDelegate)
        {
            Should.NotBeNull(traceDelegate, nameof(traceDelegate));
            _traceDelegate = traceDelegate;
        }

        #endregion

        #region Properties

        public static bool TraceInformation { get; set; }

        public static bool TraceWarning { get; set; } = Debugger.IsAttached;

        public static bool TraceError { get; set; } = Debugger.IsAttached;

        private static ITracer TracerInternal => Service<ITracer>.Instance;

        #endregion

        #region Implementation of interfaces

        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                _traceDelegate(level + ": " + message);
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

        public static void Info(string message)
        {
            TracerInternal.Trace(TraceLevel.Information, message);
        }

        public static void Warn(string message)
        {
            TracerInternal.Trace(TraceLevel.Warning, message);
        }

        public static void Error(string message)
        {
            TracerInternal.Trace(TraceLevel.Error, message);
        }

        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            TracerInternal.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(string format, params object[] args)
        {
            TracerInternal.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            TracerInternal.Trace(TraceLevel.Error, format, args);
        }

        #endregion
    }
}