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
    public sealed class AndroidConditionNavigationDispatcher : IConditionNavigationDispatcherComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public Task<bool>? CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.Target == null)
                return null;

            if (navigationContext.NavigationMode.IsNew || navigationContext.NavigationMode.IsRefresh || navigationContext.NavigationMode.IsRestore)
            {
                return navigationDispatcher
                    .WaitNavigationAsync(navigationContext.Target, navigationContext, (callback, context) =>
                        callback.NavigationType == NavigationType.Background && callback.CallbackType == NavigationCallbackType.Close ||
                        (callback.NavigationType == context.NavigationType || callback.NavigationType == NavigationType.Page) &&
                        (callback.CallbackType == NavigationCallbackType.Showing || callback.CallbackType == NavigationCallbackType.Closing), true, false, navigationContext.GetMetadataOrDefault())
                    .ContinueWith(_ => true, TaskContinuationOptions.ExecuteSynchronously);
            }

            if (navigationContext.NavigationMode.IsClose)
            {
                return navigationDispatcher
                    .WaitNavigationAsync(navigationContext.Target, navigationContext, (callback, state)
                        => callback.NavigationType == NavigationType.Background &&
                           callback.CallbackType == NavigationCallbackType.Close, true, false, navigationContext.GetMetadataOrDefault())
                    .ContinueWith(_ => true, TaskContinuationOptions.ExecuteSynchronously);
            }

            return null;
        }

        #endregion
    }
}