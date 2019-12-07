using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class BusyIndicatorComponentExtensions
    {
        #region Methods

        public static void OnBeginBusy(this IBusyIndicatorProviderListener[] listeners, IBusyIndicatorProvider provider, IBusyInfo busyInfo)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBusy(provider, busyInfo);
        }

        public static void OnBusyInfoChanged(this IBusyIndicatorProviderListener[] listeners, IBusyIndicatorProvider provider)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBusyInfoChanged(provider);
        }

        #endregion
    }
}