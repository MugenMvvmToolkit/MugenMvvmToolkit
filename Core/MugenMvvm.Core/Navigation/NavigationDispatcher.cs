using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Navigation
{
    public sealed class NavigationDispatcher : ComponentOwnerBase<INavigationDispatcher>, INavigationDispatcher
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type = null, IReadOnlyMetadataContext? metadata = null)
        {
            List<INavigationEntry>? result = null;
            var components = GetComponents<INavigationEntryProviderComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var list = components[i].TryGetNavigationEntries(type, metadata);
                if (list == null || list.Count == 0)
                    continue;

                if (result == null)
                    result = new List<INavigationEntry>();
                result.AddRange(list);
            }

            return (IReadOnlyList<INavigationEntry>?)result ?? Default.EmptyArray<INavigationEntry>();
        }

        public Task<bool> OnNavigatingAsync(INavigationContext navigationContext)
        {
            var components = GetComponents<INavigationDispatcherNavigatingListener>(navigationContext.GetMetadataOrDefault());
            if (components.Length == 0)
                return Default.TrueTask;
            if (components.Length == 1)
                return components[0].OnNavigatingAsync(this, navigationContext) ?? Default.TrueTask;
            return new NavigatingResult(this, components, navigationContext).Task;
        }

        public void OnNavigated(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var components = GetComponents<INavigationDispatcherNavigatedListener>(navigationContext.GetMetadataOrDefault());
            for (var i = 0; i < components.Length; i++)
                components[i].OnNavigated(this, navigationContext);
        }

        public void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            var components = GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault());
            for (var i = 0; i < components.Length; i++)
                components[i].OnNavigationFailed(this, navigationContext, exception);
        }

        public void OnNavigationCanceled(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            var components = GetComponents<INavigationDispatcherErrorListener>(navigationContext.GetMetadataOrDefault());
            for (var i = 0; i < components.Length; i++)
                components[i].OnNavigationCanceled(this, navigationContext);
        }

        #endregion

        #region Nested types

        private sealed class NavigatingResult : TaskCompletionSource<bool>
        {
            #region Fields

            private readonly INavigationDispatcherNavigatingListener[] _components;
            private readonly NavigationDispatcher _dispatcher;
            private readonly INavigationContext _navigationContext;
            private int _index;

            #endregion

            #region Constructors

            public NavigatingResult(NavigationDispatcher dispatcher, INavigationDispatcherNavigatingListener[] components, INavigationContext navigationContext)
            {
                _dispatcher = dispatcher;
                _components = components;
                _navigationContext = navigationContext;
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

                    if (_index >= _components.Length)
                    {
                        SetResult(true, null, false);
                        return;
                    }

                    var resultTask = _components[_index].OnNavigatingAsync(_dispatcher, _navigationContext) ?? Default.TrueTask;
                    ++_index;
                    resultTask.ContinueWith(OnExecuted, this, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception e)
                {
                    SetResult(false, e, false);
                }
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

            private static void OnExecuted(Task<bool> task, object state)
            {
                ((NavigatingResult)state).OnExecuted(task);
            }

            #endregion
        }

        #endregion
    }
}