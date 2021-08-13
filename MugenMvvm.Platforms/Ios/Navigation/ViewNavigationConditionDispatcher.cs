using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Ios.Navigation
{
    public sealed class ViewNavigationConditionDispatcher : INavigationConditionComponent, IHasPriority
    {
        public int Priority { get; init; } = NavigationComponentPriority.Condition;

        public async ValueTask<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.Target != null && (navigationContext.NavigationMode.IsNew || navigationContext.NavigationMode.IsRefresh
                                                                                            || navigationContext.NavigationMode.IsRestore ||
                                                                                            navigationContext.NavigationMode.IsClose))
            {
                await navigationDispatcher.WaitNavigationAsync(navigationContext.Target, navigationContext,
                    (callback, context) => (callback.NavigationType == context.NavigationType || callback.NavigationType == NavigationType.Page) &&
                                           (callback.CallbackType == NavigationCallbackType.Show || callback.CallbackType == NavigationCallbackType.Closing), true, false,
                    navigationContext.GetMetadataOrDefault());
            }

            return true;
        }
    }
}