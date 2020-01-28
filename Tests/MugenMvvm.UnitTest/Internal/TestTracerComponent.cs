using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal
{
    public class TestTracerComponent : ITracerComponent, IHasPriority
    {
        #region Properties

        public Func<TraceLevel, IReadOnlyMetadataContext?, bool>? CanTrace { get; set; }

        public Action<TraceLevel, string, Exception?, IReadOnlyMetadataContext?>? Trace { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ITracerComponent.CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata)
        {
            return CanTrace?.Invoke(level, metadata) ?? false;
        }

        void ITracerComponent.Trace(TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            Trace?.Invoke(level, message, exception, metadata);
        }

        #endregion
    }
}