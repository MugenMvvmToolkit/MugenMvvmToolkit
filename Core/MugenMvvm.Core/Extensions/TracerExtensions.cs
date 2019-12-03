using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class Tracer
    {
        #region Methods

        public static TracerWithLevel? GetTracerWithLevel(this ITracer? tracer, TraceLevel level)
        {
            if (tracer == null || !tracer.CanTrace(level))
                return null;
            return new TracerWithLevel(tracer, level);
        }

        public static TracerWithLevel? Info()
        {
            return MugenService.Optional<ITracer>().Info();
        }

        public static TracerWithLevel? Warn()
        {
            return MugenService.Optional<ITracer>().Warn();
        }

        public static TracerWithLevel? Error()
        {
            return MugenService.Optional<ITracer>().Error();
        }

        public static TracerWithLevel? Info(this ITracer? tracer)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Information);
        }

        public static TracerWithLevel? Warn(this ITracer? tracer)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Warning);
        }

        public static TracerWithLevel? Error(this ITracer? tracer)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Error);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, string format, params object?[] args)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            if (tracer.CanTrace(level))
                tracer.Trace(level, format.Format(args));
        }

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

        #region Nested types

        public readonly struct TracerWithLevel
        {
            #region Fields

            private readonly TraceLevel _level;
            private readonly ITracer _tracer;

            #endregion

            #region Constructors

            public TracerWithLevel(ITracer tracer, TraceLevel level)
            {
                _tracer = tracer;
                _level = level;
            }

            #endregion

            #region Methods

            public void Trace(string message)
            {
                _tracer.Trace(_level, message);
            }

            [StringFormatMethod("format")]
            public void Trace(string format, params object?[] args)
            {
                _tracer.Trace(_level, format, args);
            }

            #endregion
        }

        #endregion
    }
}