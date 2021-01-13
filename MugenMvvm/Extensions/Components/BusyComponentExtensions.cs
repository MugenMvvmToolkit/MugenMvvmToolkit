using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class BusyComponentExtensions
    {
        #region Methods

        public static IBusyToken? TryBeginBusy(this ItemOrArray<IBusyManagerComponent> components, IBusyManager busyManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            foreach (var c in components)
            {
                var token = c.TryBeginBusy(busyManager, request, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static IBusyToken? TryGetToken<TState>(this ItemOrArray<IBusyManagerComponent> components, IBusyManager busyManager, Func<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, TState state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(filter, nameof(filter));
            Should.NotBeNull(busyManager, nameof(busyManager));
            foreach (var c in components)
            {
                var token = c.TryGetToken(busyManager, filter, state, metadata);
                if (token != null)
                    return token;
            }

            return null;
        }

        public static ItemOrIReadOnlyList<IBusyToken> TryGetTokens(this ItemOrArray<IBusyManagerComponent> components, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetTokens(busyManager, metadata);
            var result = new ItemOrListEditor<IBusyToken>();
            foreach (var c in components)
                result.AddRange(c.TryGetTokens(busyManager, metadata));
            return result.ToItemOrList();
        }

        public static void OnBeginBusy(this ItemOrArray<IBusyManagerListener> listeners, IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            Should.NotBeNull(busyToken, nameof(busyToken));
            foreach (var c in listeners)
                c.OnBeginBusy(busyManager, busyToken, metadata);
        }

        public static void OnBusyStateChanged(this ItemOrArray<IBusyManagerListener> listeners, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(busyManager, nameof(busyManager));
            foreach (var c in listeners)
                c.OnBusyStateChanged(busyManager, metadata);
        }

        #endregion
    }
}