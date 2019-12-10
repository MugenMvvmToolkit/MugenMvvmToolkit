using System;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static TracerWithLevel? GetTracerWithLevel(this ITracer? tracer, TraceLevel level, IReadOnlyMetadataContext? metadata = null)
        {
            if (tracer == null || !tracer.CanTrace(level, metadata))
                return null;
            return new TracerWithLevel(tracer, level);
        }

        public static TracerWithLevel? Info(this ITracer? tracer, IReadOnlyMetadataContext? metadata = null)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Information, metadata);
        }

        public static TracerWithLevel? Warn(this ITracer? tracer, IReadOnlyMetadataContext? metadata = null)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Warning, metadata);
        }

        public static TracerWithLevel? Error(this ITracer? tracer, IReadOnlyMetadataContext? metadata = null)
        {
            return tracer.GetTracerWithLevel(TraceLevel.Error, metadata);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, string format, params object?[] args)
        {
            tracer.Trace(level, null, null, format, args);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, Exception? exception, string format, params object?[] args)
        {
            tracer.Trace(level, exception, null, format, args);
        }

        [StringFormatMethod("format")]
        public static void Trace(this ITracer tracer, TraceLevel level, Exception? exception, IReadOnlyMetadataContext? metadata, string format, params object?[] args)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            tracer.Trace(level, format.Format(args), exception, metadata);
        }

        #endregion

        #region Nested types

        public readonly struct TracerWithLevel
        {
            #region Fields

            public readonly TraceLevel TraceLevel;
            public readonly ITracer Tracer;

            #endregion

            #region Constructors

            public TracerWithLevel(ITracer tracer, TraceLevel traceLevel)
            {
                Tracer = tracer;
                TraceLevel = traceLevel;
            }

            #endregion

            #region Methods

            public void Trace(string message, IReadOnlyMetadataContext? metadata)
            {
                Tracer?.Trace(TraceLevel, message, null, metadata);
            }

            public void Trace(string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null)
            {
                Tracer?.Trace(TraceLevel, message, exception, metadata);
            }

            [StringFormatMethod("format")]
            public void Trace(string format, params object?[] args)
            {
                Tracer?.Trace(TraceLevel, format, args);
            }

            [StringFormatMethod("format")]
            public void Trace(Exception? exception, string format, params object?[] args)
            {
                Tracer?.Trace(TraceLevel, exception, format, args);
            }

            [StringFormatMethod("format")]
            public void Trace(Exception? exception, IReadOnlyMetadataContext? metadata, string format, params object?[] args)
            {
                Tracer?.Trace(TraceLevel, exception, metadata, format, args);
            }

            #endregion
        }

        #endregion
    }
}