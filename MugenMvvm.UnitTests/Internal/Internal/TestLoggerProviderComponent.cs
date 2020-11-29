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
        #region Fields

        private readonly ILogger? _logger;

        #endregion

        #region Constructors

        public TestLoggerProviderComponent(ILogger? logger = null)
        {
            _logger = logger;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, ILogger?>? TryGetLogger { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ILogger? ILoggerProviderComponent.TryGetLogger(ILogger logger, object request, IReadOnlyMetadataContext? metadata)
        {
            _logger?.ShouldEqual(logger);
            return TryGetLogger?.Invoke(request, metadata);
        }

        #endregion
    }
}