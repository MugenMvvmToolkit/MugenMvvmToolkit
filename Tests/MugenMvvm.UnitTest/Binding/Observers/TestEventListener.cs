using System;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class TestEventListener : IEventListener
    {
        #region Properties

        public bool IsAlive { get; set; } = true;

        public bool IsWeak { get; set; }

        public Func<object, object?, bool>? TryHandle { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object sender, object? message)
        {
            return TryHandle?.Invoke(sender, message) ?? true;
        }

        #endregion
    }
}