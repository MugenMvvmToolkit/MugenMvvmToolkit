using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class BusyIndicatorComponentExtensions
    {
        #region Methods

        public static void OnBeginBusy(this IBusyIndicatorProvider provider, IBusyInfo busyInfo, IBusyIndicatorProviderListener[]? listeners = null)
        {
            Should.NotBeNull(provider, nameof(provider));
            if (listeners == null)
                listeners = provider.GetComponents<IBusyIndicatorProviderListener>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBusy(provider, busyInfo);
        }

        public static void OnBusyInfoChanged(this IBusyIndicatorProvider provider, IBusyIndicatorProviderListener[]? listeners = null)
        {
            Should.NotBeNull(provider, nameof(provider));
            if (listeners == null)
                listeners = provider.GetComponents<IBusyIndicatorProviderListener>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBusyInfoChanged(provider);
        }

        #endregion
    }
}