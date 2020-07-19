using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        public static IBusyToken? TryGetToken(this IBusyManagerComponent[] components, IBusyManager busyManager, Func<object?, IBusyToken, IReadOnlyMetadataContext?, bool> filter, object? state, IReadOnlyMetadataContext? metadata)
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

        public static ItemOrList<IBusyToken, IReadOnlyList<IBusyToken>> TryGetTokens(this IBusyManagerComponent[] components, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (components.Length == 1)
                return components[0].TryGetTokens(busyManager, metadata);
            ItemOrListEditor<IBusyToken, List<IBusyToken>> result = ItemOrListEditor.Get<IBusyToken>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetTokens(busyManager, metadata));
            return result.ToItemOrList<IReadOnlyList<IBusyToken>>();
        }

        public static void OnBeginBusy(this IBusyManagerListener[] listeners, IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(busyManager, nameof(busyManager));
            Should.NotBeNull(busyToken, nameof(busyToken));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBusy(busyManager, busyToken, metadata);
        }

        public static void OnBusyChanged(this IBusyManagerListener[] listeners, IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(busyManager, nameof(busyManager));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBusyChanged(busyManager, metadata);
        }

        #endregion
    }
}