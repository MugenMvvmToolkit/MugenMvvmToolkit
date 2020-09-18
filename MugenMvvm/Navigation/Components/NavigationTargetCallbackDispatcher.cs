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
    public sealed class NavigationTargetCallbackDispatcher : IConditionNavigationDispatcherComponent, INavigationDispatcherNavigatingListener, INavigationDispatcherNavigatedListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public async Task<bool>? CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var prevTarget = GetPrevNavigationTarget(navigationDispatcher, navigationContext);
            var target = navigationContext.Target;
            if (navigationContext.NavigationMode.IsClose)
            {
                return await CanNavigateFromAsync(target, prevTarget, navigationContext, cancellationToken).ConfigureAwait(false) &&
                       await CanNavigateToAsync(prevTarget, target, navigationContext, cancellationToken).ConfigureAwait(false);
            }

            return await CanNavigateFromAsync(prevTarget, target, navigationContext, cancellationToken).ConfigureAwait(false) &&
                   await CanNavigateToAsync(target, prevTarget, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var prevTarget = GetPrevNavigationTarget(navigationDispatcher, navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatedCallback)?.OnNavigatedFrom(prevTarget, navigationContext);
                (prevTarget as IHasNavigatedCallback)?.OnNavigatedTo(navigationContext.Target, navigationContext);
            }
            else
            {
                (prevTarget as IHasNavigatedCallback)?.OnNavigatedFrom(navigationContext.Target, navigationContext);
                (navigationContext.Target as IHasNavigatedCallback)?.OnNavigatedTo(prevTarget, navigationContext);
            }
        }

        public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var prevTarget = GetPrevNavigationTarget(navigationDispatcher, navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatingCallback)?.OnNavigatingFrom(prevTarget, navigationContext);
                (prevTarget as IHasNavigatingCallback)?.OnNavigatingTo(navigationContext.Target, navigationContext);
            }
            else
            {
                (prevTarget as IHasNavigatingCallback)?.OnNavigatingFrom(navigationContext.Target, navigationContext);
                (navigationContext.Target as IHasNavigatingCallback)?.OnNavigatingTo(prevTarget, navigationContext);
            }
        }

        #endregion

        #region Methods

        private static object? GetPrevNavigationTarget(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext) =>
            navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.NavigationType == context.NavigationType && entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            }) ?? navigationDispatcher.GetTopNavigation(navigationContext, (entry, context, m) =>
            {
                if (entry.Target != null && !Equals(entry.Target, context.Target))
                    return entry.Target;
                return null;
            });

        private static Task<bool> CanNavigateFromAsync(object? target, object? toTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (target is IHasNavigationCondition condition)
                return condition.CanNavigateFromAsync(toTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            return Default.TrueTask;
        }

        private static Task<bool> CanNavigateToAsync(object? target, object? fromTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (target is IHasNavigationCondition condition)
                return condition.CanNavigateToAsync(fromTarget, navigationContext, cancellationToken) ?? Default.TrueTask;
            return Default.TrueTask;
        }

        #endregion
    }
}