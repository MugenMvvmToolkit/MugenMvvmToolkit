using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;

namespace MugenMvvm.Internal
{
    public sealed class Logger : ComponentOwnerBase<ILogger>, ILogger
    {
        public Logger(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILogger Get(object request, IReadOnlyMetadataContext? metadata = null) => MugenService.Instance<ILogger>().GetLogger(request, metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Trace(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Trace(metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Debug(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Debug(metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Info(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Info(metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Warn(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Warn(metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Error(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Error(metadata);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MugenExtensions.LoggerWithLevel? Fatal(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ILogger>().Fatal(metadata);

        public void Dispose()
        {
            var components = GetComponents<IDisposableComponent<ILogger>>();
            components.OnDisposing(this, null);
            components.OnDisposed(this, null);
        }

        public ILogger? TryGetLogger(object request, IReadOnlyMetadataContext? metadata = null) => GetComponents<ILoggerProviderComponent>().TryGetLogger(this, request, metadata);

        public bool CanLog(LogLevel level, IReadOnlyMetadataContext? metadata = null) => GetComponents<ILoggerComponent>(metadata).CanLog(this, level, metadata);

        public void Log(LogLevel level, object message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ILoggerComponent>(metadata).Log(this, level, message, exception, metadata);
    }
}