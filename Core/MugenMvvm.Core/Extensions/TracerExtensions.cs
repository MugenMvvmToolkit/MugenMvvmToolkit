using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static void Info(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Information, message);
        }

        public static void Warn(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Warning, message);
        }

        public static void Error(this ITracer tracer, string message)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(TraceLevel.Error, message);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, string format, params object?[] args)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            if (tracer.CanTrace(level))
                tracer.Trace(level, format.Format(args));
        }

        [StringFormatMethod("format")]
        public static void Info(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(this ITracer tracer, string format, params object?[] args)
        {
            tracer.Trace(TraceLevel.Error, format, args);
        }

        #endregion
    }
}