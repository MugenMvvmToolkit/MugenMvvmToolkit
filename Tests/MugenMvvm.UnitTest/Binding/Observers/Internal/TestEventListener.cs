using System;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.UnitTest.Binding.Observers.Internal
{
    public class TestEventListener : IEventListener
    {
        #region Properties

        public bool IsAlive { get; set; } = true;

        public bool IsWeak { get; set; }

        public int InvokeCount { get; set; }

        public Func<object?, object?, bool>? TryHandle { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle<T>(object? sender, in T message)
        {
            ++InvokeCount;
            return TryHandle?.Invoke(sender, message) ?? true;
        }

        #endregion
    }
}