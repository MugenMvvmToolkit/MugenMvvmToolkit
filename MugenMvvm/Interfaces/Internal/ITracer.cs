using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ITracer : IComponentOwner<ITracer>
    {
        bool CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata = null);

        void Trace(TraceLevel level, string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null);
    }
}