using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationCallbackTargetDispatcher : INavigationConditionComponent, INavigationListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public async Task<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            var target = navigationContext.Target;
            if (navigationContext.NavigationMode.IsClose)
            {
                return await CanNavigateFromAsync(navigationDispatcher, target, nextTarget, navigationContext, cancellationToken).ConfigureAwait(false) &&
                       await CanNavigateToAsync(navigationDispatcher, nextTarget, target, navigationContext, cancellationToken).ConfigureAwait(false);
            }

            return await CanNavigateFromAsync(navigationDispatcher, nextTarget, target, navigationContext, cancellationToken).ConfigureAwait(false) &&
                   await CanNavigateToAsync(navigationDispatcher, target, nextTarget, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatingCallback)?.OnNavigatingFrom(navigationDispatcher, navigationContext, nextTarget);
                (nextTarget as IHasNavigatingCallback)?.OnNavigatingTo(navigationDispatcher, navigationContext, navigationContext.Target);
            }
            else
            {
                (nextTarget as IHasNavigatingCallback)?.OnNavigatingFrom(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasNavigatingCallback)?.OnNavigatingTo(navigationDispatcher, navigationContext, nextTarget);
            }
        }

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatedCallback)?.OnNavigatedFrom(navigationDispatcher, navigationContext, nextTarget);
                (nextTarget as IHasNavigatedCallback)?.OnNavigatedTo(navigationDispatcher, navigationContext, navigationContext.Target);
            }
            else
            {
                (nextTarget as IHasNavigatedCallback)?.OnNavigatedFrom(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasNavigatedCallback)?.OnNavigatedTo(navigationDispatcher, navigationContext, nextTarget);
            }
        }

        #endregion

        #region Methods

        private static Task<bool> CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, object? target, object? toTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
            => (target as IHasNavigationConditionCallback)?.CanNavigateFromAsync(navigationDispatcher, navigationContext, toTarget, cancellationToken) ?? Default.TrueTask;

        private static Task<bool> CanNavigateToAsync(INavigationDispatcher navigationDispatcher, object? target, object? fromTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
            => (target as IHasNavigationConditionCallback)?.CanNavigateToAsync(navigationDispatcher, navigationContext, fromTarget, cancellationToken) ?? Default.TrueTask;

        #endregion
    }
}