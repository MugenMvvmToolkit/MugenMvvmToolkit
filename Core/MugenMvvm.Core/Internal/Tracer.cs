using System;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Internal;
using TraceLevel = MugenMvvm.Enums.TraceLevel;

namespace MugenMvvm.Internal
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

        public static bool TraceWarning { get; set; }

        public static bool TraceError { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                _traceDelegate(level + ": " + message);
        }

        public bool CanTrace(TraceLevel level)
        {
            if (level == TraceLevel.Information)
                return TraceInformation;
            if (level == TraceLevel.Warning)
                return TraceWarning;
            return level == TraceLevel.Error;
        }

        #endregion

        #region Methods

        public static void Info(string message)
        {
            MugenService.Tracer.Trace(TraceLevel.Information, message);
        }

        public static void Warn(string message)
        {
            MugenService.Tracer.Trace(TraceLevel.Warning, message);
        }

        public static void Error(string message)
        {
            MugenService.Tracer.Trace(TraceLevel.Error, message);
        }

        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            MugenService.Tracer.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(string format, params object[] args)
        {
            MugenService.Tracer.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            MugenService.Tracer.Trace(TraceLevel.Error, format, args);
        }

        #endregion
    }
}