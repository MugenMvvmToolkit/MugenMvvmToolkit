using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestSuspendableComponent : ISuspendable
    {
        #region Properties

        public bool IsSuspended { get; set; }

        public Func<ActionToken>? Suspend { get; set; }

        #endregion

        #region Implementation of interfaces

        ActionToken ISuspendable.Suspend()
        {
            return Suspend?.Invoke() ?? default;
        }

        #endregion
    }
}