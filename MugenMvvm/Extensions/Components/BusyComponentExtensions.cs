using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class BusyComponentExtensions
    {
        #region Methods

        public static IBusyToken? TryBeginBusy(this IBusyManagerComponent[] components, IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(busyManager, nameof(busyManager));
            for (var i = 0; i < components.Length; i++)
            {
                var token = components[i].TryBeginBusy(busyManager, request, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static IBusyToken? TryGetToken<TState>(this IBusyManagerComponent[] components, IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(filter, nameof(filter));
            Should.NotBeNull(busyManager, nameof(busyManager));
            for (var i = 0; i < components.Length; i++)
            {
                var token = components[i].TryGetToken(busyManager, filter, state, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static ItemOrIReadOnlyList<IBusyToken> TryGetTokens(this IBusyManagerComponent[] components, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (components.Length == 1)
                return components[0].TryGetTokens(busyManager, metadata);
            var result = new ItemOrListEditor<IBusyToken>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetTokens(busyManager, metadata));
            return result.ToItemOrList();
        }

        public static void OnBeginBusy(this IBusyManagerListener[] listeners, IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(busyManager, nameof(busyManager));
            Should.NotBeNull(busyToken, nameof(busyToken));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBusy(busyManager, busyToken, metadata);
        }

        public static void OnBusyStateChanged(this IBusyManagerListener[] listeners, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(busyManager, nameof(busyManager));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBusyStateChanged(busyManager, metadata);
        }

        #endregion
    }
}