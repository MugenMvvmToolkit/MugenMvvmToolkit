using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTest.Internal;

namespace MugenMvvm.UnitTest.Busy
{
    public sealed class TestBusyManagerComponent : SuspendableComponent, IBusyManagerComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext, IBusyToken?> TryBeginBusy { get; set; }

        public Func<FuncIn<object, IBusyToken, IReadOnlyMetadataContext?, bool>, object, Type, Delegate, IReadOnlyMetadataContext, IBusyToken?> TryGetToken { get; set; }

        public Func<IReadOnlyMetadataContext, IReadOnlyList<IBusyToken>?> TryGetTokens { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IBusyToken? IBusyManagerComponent.TryBeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryBeginBusy?.Invoke(request, typeof(TRequest), metadata);
        }

        IBusyToken? IBusyManagerComponent.TryGetToken<TState>(in TState state, FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata)
        {
            return TryGetToken?.Invoke((in object o, IBusyToken token, IReadOnlyMetadataContext? arg3) => filter((TState) o, token, arg3), state, typeof(TState), filter, metadata);
        }

        IReadOnlyList<IBusyToken>? IBusyManagerComponent.TryGetTokens(IReadOnlyMetadataContext? metadata)
        {
            return TryGetTokens?.Invoke(metadata);
        }

        #endregion
    }
}