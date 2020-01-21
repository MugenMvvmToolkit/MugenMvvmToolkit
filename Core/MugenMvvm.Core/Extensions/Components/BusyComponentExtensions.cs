using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class BusyComponentExtensions
    {
        #region Methods

        public static IBusyToken? TryBeginBusy<TRequest>(this IBusyManagerComponent[] components, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var token = components[i].TryBeginBusy(request, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static IBusyToken? TryGetToken<TState>(this IBusyManagerComponent[] components, in TState state, FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(filter, nameof(filter));
            for (var i = 0; i < components.Length; i++)
            {
                var token = components[i].TryGetToken(state, filter, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static IReadOnlyList<IBusyToken>? TryGetTokens(this IBusyManagerComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            LazyList<IBusyToken> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetTokens(metadata));
            return result.List;
        }

        public static void OnBeginBusy(this IBusyManagerListener[] listeners, IBusyManager provider, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(busyToken, nameof(busyToken));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBusy(provider, busyToken, metadata);
        }

        public static void OnBusyChanged(this IBusyManagerListener[] listeners, IBusyManager provider, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(provider, nameof(provider));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBusyChanged(provider, metadata);
        }

        #endregion
    }
}