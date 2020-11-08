using System;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class Tracer : ComponentOwnerBase<ITracer>, ITracer
    {
        #region Constructors

        public Tracer(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata = null) => GetComponents<ITracerComponent>(metadata).CanTrace(this, level, metadata);

        public void Trace(TraceLevel level, string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<ITracerComponent>(metadata).Trace(this, level, message, exception, metadata);

        #endregion

        #region Methods

        public static MugenExtensions.TracerWithLevel? Info(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ITracer>().Info(metadata);

        public static MugenExtensions.TracerWithLevel? Warn(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ITracer>().Warn(metadata);

        public static MugenExtensions.TracerWithLevel? Error(IReadOnlyMetadataContext? metadata = null) => MugenService.Optional<ITracer>().Error(metadata);

        #endregion
    }
}