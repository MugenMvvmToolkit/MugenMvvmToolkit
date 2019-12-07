using System.Threading.Tasks;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static TTask WithBusyIndicator<TTask>(this TTask task, IHasService<IBusyIndicatorProvider> busyIndicatorProvider, object? message = null, int millisecondsDelay = 0)
            where TTask : Task
        {
            Should.NotBeNull(busyIndicatorProvider, nameof(busyIndicatorProvider));
            return task.WithBusyIndicator(busyIndicatorProvider.Service, message, millisecondsDelay);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IBusyIndicatorProvider busyIndicatorProvider, object? message = null, int millisecondsDelay = 0)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(busyIndicatorProvider, nameof(busyIndicatorProvider));
            if (task.IsCompleted)
                return task;
            if (millisecondsDelay == 0 && message is IHasBusyDelayMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            var token = busyIndicatorProvider.Begin(message, millisecondsDelay);
            task.ContinueWith((t, o) => ((IBusyToken) o).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static void ClearBusy(this IBusyIndicatorProvider provider)
        {
            Should.NotBeNull(provider, nameof(provider));
            var tokens = provider.GetTokens();
            for (var i = 0; i < tokens.Count; i++)
                tokens[i].Dispose();
        }

        #endregion
    }
}