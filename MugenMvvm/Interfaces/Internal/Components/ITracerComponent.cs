using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ITracerComponent : IComponent<ITracer>
    {
        bool CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata);

        void Trace(TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata);
    }
}