using System;
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

        public Task<bool>? CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var prevTarget = GetPrevNavigationTarget(navigationDispatcher, navigationContext);
            var target = navigationContext.Target;
            if (prevTarget is IHasNavigationCondition c1 && target is IHasNavigationCondition c2)
                return new CanNavigateResult(c1, c2, navigationContext, cancellationToken).Task;

            if (navigationContext.NavigationMode.IsClose)
            {
                if (navigationContext.Target is IHasNavigationCondition condition)
                    return condition.CanNavigateFromAsync(prevTarget, navigationContext, cancellationToken);
                return (prevTarget as IHasNavigationCondition)?.CanNavigateToAsync(navigationContext.Target, navigationContext, cancellationToken);
            }

            if (prevTarget is IHasNavigationCondition c)
                return c.CanNavigateFromAsync(navigationContext.Target, navigationContext, cancellationToken);
            return (navigationContext.Target as IHasNavigationCondition)?.CanNavigateToAsync(prevTarget, navigationContext, cancellationToken);
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

        #endregion

        #region Nested types

        private sealed class CanNavigateResult : TaskCompletionSource<bool>
        {
            #region Fields

            private readonly CancellationToken _cancellationToken;
            private readonly INavigationContext _navigationContext;

            private readonly IHasNavigationCondition _prevTarget;
            private readonly IHasNavigationCondition _target;
            private int _state;

            #endregion

            #region Constructors

            public CanNavigateResult(IHasNavigationCondition prevTarget, IHasNavigationCondition target, INavigationContext navigationContext, CancellationToken cancellationToken)
            {
                _prevTarget = prevTarget;
                _target = target;
                _navigationContext = navigationContext;
                _cancellationToken = cancellationToken;
                OnExecuted(Default.TrueTask);
            }

            #endregion

            #region Methods

            private void OnExecuted(Task<bool> task)
            {
                try
                {
                    if (task.IsCanceled)
                    {
                        SetResult(false, null, true);
                        return;
                    }

                    if (!task.Result)
                    {
                        SetResult(false, null, false);
                        return;
                    }

                    if (_state == 2)
                    {
                        SetResult(true, null, false);
                        return;
                    }

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        SetResult(false, null, true);
                        return;
                    }

                    ++_state;
                    var resultTask = CanNavigateAsync();
                    if (resultTask == null)
                        OnExecuted(Default.TrueTask);
                    else
                        resultTask.ContinueWith((t, state) => ((CanNavigateResult)state!).OnExecuted(t), this, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception e)
                {
                    SetResult(false, e, false);
                }
            }

            private Task<bool>? CanNavigateAsync()
            {
                if (_navigationContext.NavigationMode.IsClose)
                {
                    if (_state == 1)
                        return _target.CanNavigateFromAsync(_prevTarget, _navigationContext, _cancellationToken);
                    return _prevTarget.CanNavigateToAsync(_target, _navigationContext, _cancellationToken);
                }

                if (_state == 1)
                    return _prevTarget.CanNavigateFromAsync(_target, _navigationContext, _cancellationToken);
                return _target.CanNavigateToAsync(_prevTarget, _navigationContext, _cancellationToken);
            }

            private void SetResult(bool result, Exception? exception, bool canceled)
            {
                if (exception != null)
                    this.TrySetExceptionEx(exception);
                else if (canceled)
                    TrySetCanceled();
                else
                    TrySetResult(result);
            }

            #endregion
        }

        #endregion
    }
}