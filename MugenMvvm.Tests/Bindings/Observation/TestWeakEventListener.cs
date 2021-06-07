using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Observation
{
    public class TestWeakEventListener : IWeakEventListener
    {
        public int InvokeCount { get; set; }

        public bool TryHandleDefault { get; set; } = true;

        public Func<object?, object?, IReadOnlyMetadataContext?, bool>? TryHandle { get; set; }

        public bool IsWeak { get; set; }

        public bool IsAlive { get; set; } = true;

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            ++InvokeCount;
            return TryHandle?.Invoke(sender, message, metadata) ?? TryHandleDefault;
        }
    }
}