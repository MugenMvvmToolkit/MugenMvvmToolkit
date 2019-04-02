using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcher : INavigationDispatcher
    {
        #region Fields

        private IComponentCollection<INavigationDispatcherListener>? _listeners;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(INavigationContextFactory contextFactory, INavigationDispatcherJournal navigationJournal,
            IComponentCollection<INavigationDispatcherListener>? listeners = null)
        {
            Should.NotBeNull(contextFactory, nameof(contextFactory));
            Should.NotBeNull(navigationJournal, nameof(navigationJournal));
            ContextFactory = contextFactory;
            NavigationJournal = navigationJournal;
            _listeners = listeners;
            contextFactory.OnAttached(this, Default.MetadataContext);
            navigationJournal.OnAttached(this, Default.MetadataContext);
        }

        #endregion

        #region Properties

        public INavigationContextFactory ContextFactory { get; }

        public INavigationDispatcherJournal NavigationJournal { get; }

        public IComponentCollection<INavigationDispatcherListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public INavigatingResult OnNavigating(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            return OnNavigatingInternal(navigationContext);
        }

        public void OnNavigated(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            NavigationJournal.OnNavigated(navigationContext);
            OnNavigatedInternal(navigationContext);
        }

        public void OnNavigationFailed(INavigationContext navigationContext, Exception exception)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(navigationContext, exception);
        }

        public void OnNavigationCanceled(INavigationContext navigationContext)
        {
            Should.NotBeNull(navigationContext, nameof(navigationContext));
            OnNavigationCanceledInternal(navigationContext);
        }

        #endregion

        #region Methods

        protected virtual INavigatingResult OnNavigatingInternal(INavigationContext navigationContext)
        {
            return new NavigatingResult(this, GetListeners(), navigationContext);
        }

        protected virtual void OnNavigatedInternal(INavigationContext navigationContext)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnNavigated(this, navigationContext);
        }

        protected virtual void OnNavigationFailedInternal(INavigationContext navigationContext, Exception exception)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnNavigationFailed(this, navigationContext, exception);
        }

        protected virtual void OnNavigationCanceledInternal(INavigationContext navigationContext)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnNavigationCanceled(this, navigationContext);
        }

        protected virtual void OnNavigatingCanceledInternal(INavigationContext navigationContext)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnNavigatingCanceled(this, navigationContext);
        }

        protected IReadOnlyList<INavigationDispatcherListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<INavigationDispatcherListener>();
        }

        #endregion

        #region Nested types

        protected sealed class NavigatingResult : TaskCompletionSource<bool>, INavigatingResult
        {
            #region Fields

            private readonly NavigationDispatcher _dispatcher;
            private readonly IReadOnlyList<INavigationDispatcherListener> _listeners;

            private readonly INavigationContext _navigationContext;
            private Action<INavigationDispatcher, INavigationContext, Exception?>? _canceledCallback;
            private Func<INavigationDispatcher, INavigationContext, bool> _completeNavigationCallback;
            private int _index;

            #endregion

            #region Constructors

            public NavigatingResult(NavigationDispatcher dispatcher, IReadOnlyList<INavigationDispatcherListener> listeners, INavigationContext navigationContext)
            {
                _dispatcher = dispatcher;
                _listeners = listeners;
                _navigationContext = navigationContext;
                OnExecuted(Default.TrueTask);
            }

            #endregion

            #region Implementation of interfaces

            public Task<bool> GetResultAsync()
            {
                return Task;
            }

            public void CompleteNavigation(Func<INavigationDispatcher, INavigationContext, bool> completeNavigationCallback,
                Action<INavigationDispatcher, INavigationContext, Exception?>? canceledCallback = null)
            {
                Should.NotBeNull(completeNavigationCallback, nameof(completeNavigationCallback));
                if (Interlocked.Exchange(ref _completeNavigationCallback, completeNavigationCallback) != null)
                    ExceptionManager.ThrowNavigatingResultHasCallback();

                _canceledCallback = canceledCallback;
                Task.ContinueWith(InvokeCompletedCallback, this, TaskContinuationOptions.ExecuteSynchronously);
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

                    if (_index >= _listeners.Count)
                    {
                        SetResult(true, null, false);
                        return;
                    }

                    var resultTask = _listeners[_index].OnNavigatingAsync(_dispatcher, _navigationContext) ?? Default.TrueTask;
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
                {
                    TrySetResult(result);
                    if (!result)
                        _dispatcher.OnNavigatingCanceledInternal(_navigationContext);
                }
            }

            private void InvokeCompletedCallback(Task<bool> task)
            {
                try
                {
                    if (task.IsCanceled)
                    {
                        _canceledCallback?.Invoke(_dispatcher, _navigationContext, null);
                        _dispatcher.OnNavigationCanceled(_navigationContext);
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        _canceledCallback?.Invoke(_dispatcher, _navigationContext, task.Exception);
                        _dispatcher.OnNavigationFailed(_navigationContext, task.Exception);
                        return;
                    }

                    if (task.Result)
                    {
                        if (_completeNavigationCallback(_dispatcher, _navigationContext))
                            _dispatcher.OnNavigated(_navigationContext);
                    }
                    else
                    {
                        _canceledCallback?.Invoke(_dispatcher, _navigationContext, null);
                        _dispatcher.OnNavigationCanceled(_navigationContext);
                    }
                }
                catch (Exception e)
                {
                    _canceledCallback?.Invoke(_dispatcher, _navigationContext, e);
                    _dispatcher.OnNavigationFailed(_navigationContext, e);
                }
                finally
                {
                    _canceledCallback = null;
                    _completeNavigationCallback = (dispatcher, context) => false;
                }
            }

            private static void InvokeCompletedCallback(Task<bool> task, object state)
            {
                ((NavigatingResult) state).InvokeCompletedCallback(task);
            }

            private static void OnExecuted(Task<bool> task, object state)
            {
                ((NavigatingResult) state).OnExecuted(task);
            }

            #endregion
        }

        #endregion
    }
}