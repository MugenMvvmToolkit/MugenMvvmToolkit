using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public sealed class TestLoggerProviderComponent : ILoggerProviderComponent, IHasPriority
    {
        public Func<ILogger, object, IReadOnlyMetadataContext?, ILogger?>? TryGetLogger { get; set; }

        public int Priority { get; set; }

        ILogger? ILoggerProviderComponent.TryGetLogger(ILogger logger, object request, IReadOnlyMetadataContext? metadata) => TryGetLogger?.Invoke(logger, request, metadata);
    }
}