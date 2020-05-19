using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestSuspendableComponent : ISuspendable
    {
        #region Properties

        public bool IsSuspended { get; set; }

        public Func<object?, Type, IReadOnlyMetadataContext?, ActionToken>? Suspend { get; set; }

        #endregion

        #region Implementation of interfaces

        ActionToken ISuspendable.Suspend<TState>(in TState state, IReadOnlyMetadataContext? metadata)
        {
            return Suspend?.Invoke(state, typeof(TState), metadata) ?? default;
        }

        #endregion
    }
}