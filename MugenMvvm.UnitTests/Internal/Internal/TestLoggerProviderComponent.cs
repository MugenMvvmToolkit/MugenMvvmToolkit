using System;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public sealed class TestLoggerProviderComponent : ILoggerProviderComponent, IHasPriority
    {
        private readonly ILogger? _logger;

        public TestLoggerProviderComponent(ILogger? logger = null)
        {
            _logger = logger;
        }

        public Func<object, IReadOnlyMetadataContext?, ILogger?>? TryGetLogger { get; set; }

        public int Priority { get; set; }

        ILogger? ILoggerProviderComponent.TryGetLogger(ILogger logger, object request, IReadOnlyMetadataContext? metadata)
        {
            _logger?.ShouldEqual(logger);
            return TryGetLogger?.Invoke(request, metadata);
        }
    }
}