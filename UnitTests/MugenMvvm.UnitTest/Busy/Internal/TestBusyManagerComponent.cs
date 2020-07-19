using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;

namespace MugenMvvm.UnitTest.Busy.Internal
{
    public sealed class TestBusyManagerComponent : TestSuspendableComponent, IBusyManagerComponent, IHasPriority
    {
        #region Fields

        private readonly IBusyManager? _owner;

        #endregion

        #region Constructors

        public TestBusyManagerComponent(IBusyManager? owner = null)
        {
            _owner = owner;
        }

        #endregion

        #region Properties

        public Func<object?, IReadOnlyMetadataContext?, IBusyToken?>? TryBeginBusy { get; set; }

        public Func<Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool>, object?, IReadOnlyMetadataContext?, IBusyToken?>? TryGetToken { get; set; }

        public Func<IReadOnlyMetadataContext?, ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>>>? TryGetTokens { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IBusyToken? IBusyManagerComponent.TryBeginBusy(IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryBeginBusy?.Invoke(request, metadata);
        }

        IBusyToken? IBusyManagerComponent.TryGetToken<TState>(IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryGetToken?.Invoke((o, token, arg3) => filter((TState) o!, token, arg3), state, metadata);
        }

        ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> IBusyManagerComponent.TryGetTokens(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            _owner?.ShouldEqual(busyManager);
            return TryGetTokens?.Invoke(metadata) ?? default;
        }

        #endregion
    }
}