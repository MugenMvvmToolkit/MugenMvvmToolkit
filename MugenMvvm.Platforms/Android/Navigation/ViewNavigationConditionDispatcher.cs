using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Android.Navigation
{
    public sealed class ViewNavigationConditionDispatcher : INavigationConditionComponent, IHasPriority
    {
        public int Priority { get; init; } = NavigationComponentPriority.Condition;

        public async ValueTask<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.Target == null)
                return true;

            if (navigationContext.NavigationMode.IsNew || navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsRestore)
            {
                await navigationDispatcher
                      .WaitNavigationAsync(navigationContext.Target, navigationContext, (callback, context) =>
                              callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close ||
                              (callback.NavigationType == context.NavigationType || callback.NavigationType == NavigationType.Page) &&
                              (callback.CallbackType == NavigationCallbackType.Show || callback.CallbackType == NavigationCallbackType.Closing), true, false,
                          navigationContext.GetMetadataOrDefault())
                      .ConfigureAwait(false);
            }
            else if (navigationContext.NavigationMode.IsClose)
            {
                await navigationDispatcher
                    .WaitNavigationAsync(navigationContext.Target, navigationContext,
                        (callback, state) => callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close,
                        true, false, navigationContext.GetMetadataOrDefault());
            }

            return true;
        }
    }
}