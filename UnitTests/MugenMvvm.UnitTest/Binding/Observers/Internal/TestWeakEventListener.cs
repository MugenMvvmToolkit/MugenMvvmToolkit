using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Observers.Internal
{
    public class TestWeakEventListener : IWeakEventListener
    {
        #region Properties

        public bool IsAlive { get; set; } = true;

        public bool IsWeak { get; set; }

        public int InvokeCount { get; set; }

        public bool TryHandleDefault { get; set; } = true;

        public Func<object?, object?, IReadOnlyMetadataContext?, bool>? TryHandle { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            ++InvokeCount;
            return TryHandle?.Invoke(sender, message, metadata) ?? TryHandleDefault;
        }

        #endregion
    }
}