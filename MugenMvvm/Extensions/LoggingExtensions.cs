using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILogger GetLogger(this ILogger logger, object request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(logger, nameof(logger));
            return logger.TryGetLogger(request, metadata) ?? logger;
        }

        public static LoggerWithLevel? WithLevel(this ILogger? logger, LogLevel level, IReadOnlyMetadataContext? metadata = null)
        {
            if (logger == null || !logger.CanLog(level, metadata))
                return null;
            return new LoggerWithLevel(logger, level);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Trace(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Trace, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Debug(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Debug, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Info(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Info, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Warn(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Warning, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Error(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Error, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LoggerWithLevel? Fatal(this ILogger? logger, IReadOnlyMetadataContext? metadata = null) => logger.WithLevel(LogLevel.Fatal, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Log(this ILogger? logger, LogLevel level, string format, params object?[] args) => logger.Log(level, null, null, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Log(this ILogger? logger, LogLevel level, Exception? exception, string format, params object?[] args) =>
            logger.Log(level, exception, null, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [StringFormatMethod("format")]
        public static void Log(this ILogger? logger, LogLevel level, Exception? exception, IReadOnlyMetadataContext? metadata, string format, params object?[] args) =>
            logger?.Log(level, format.Format(args), exception, metadata);

        [StructLayout(LayoutKind.Auto)]
        public readonly struct LoggerWithLevel
        {
            public readonly LogLevel LogLevel;
            public readonly ILogger? Logger;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public LoggerWithLevel(ILogger logger, LogLevel logLevel)
            {
                Logger = logger;
                LogLevel = logLevel;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Log(object message, IReadOnlyMetadataContext? metadata) => Logger?.Log(LogLevel, message, null, metadata);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Log(object message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null) => Logger?.Log(LogLevel, message, exception, metadata);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Log(string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null) => Logger?.Log(LogLevel, message, exception, metadata);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [StringFormatMethod("format")]
            public void Log(string format, params object?[] args) => Logger?.Log(LogLevel, format, args);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [StringFormatMethod("format")]
            public void Log(Exception? exception, string format, params object?[] args) => Logger?.Log(LogLevel, exception, format, args);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [StringFormatMethod("format")]
            public void Log(Exception? exception, IReadOnlyMetadataContext? metadata, string format, params object?[] args) =>
                Logger?.Log(LogLevel, exception, metadata, format, args);
        }
    }
}