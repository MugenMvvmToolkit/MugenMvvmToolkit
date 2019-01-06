using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationDispatcher : HasListenersBase<INavigationDispatcherListener>, INavigationDispatcher
    {
        #region Fields

        protected readonly Dictionary<NavigationType, List<WeakNavigationEntry>> OpenedViewModels;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationDispatcher(ITracer tracer)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            Tracer = tracer;
            OpenedViewModels = new Dictionary<NavigationType, List<WeakNavigationEntry>>();
        }

        #endregion

        #region Properties

        protected ITracer Tracer { get; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<INavigationEntry> GetNavigationEntries(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetNavigationEntriesInternal(type, metadata);
        }

        public Task<bool> OnNavigatingAsync(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            Trace(nameof(OnNavigatingAsync), context);
            return OnNavigatingInternalAsync(context);
        }

        public void OnNavigated(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            HandleOpenedViewModels(context);
            OnNavigatedInternal(context);
            Trace(nameof(OnNavigated), context);
        }

        public void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(exception, nameof(exception));
            OnNavigationFailedInternal(context, exception);
            Trace(nameof(OnNavigationFailed), context);
        }

        public void OnNavigationCanceled(INavigationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            OnNavigationCanceledInternal(context);
            Trace(nameof(OnNavigationCanceled), context);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<INavigationEntry> GetNavigationEntriesInternal(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            lock (OpenedViewModels)
            {
                List<INavigationEntry>? result = null;
                if (type == null)
                {
                    var array = OpenedViewModels.Keys.ToArray();
                    for (var i = 0; i < array.Length; i++)
                        GetOpenedViewModelsInternal(array[i], ref result);
                }
                else
                    GetOpenedViewModelsInternal(type, ref result);

                if (result == null)
                    return Default.EmptyArray<INavigationEntry>();
                return result;
            }
        }

        protected virtual Task<bool> OnNavigatingInternalAsync(INavigationContext context)
        {
            var listeners = GetListeners()?.Where(listener => listener != null).ToArray();
            if (listeners == null || listeners.Length == 0)
                return Default.TrueTask;
            if (listeners.Length == 1)
                return listeners[0].OnNavigatingAsync(context);
            var invoker = new NavigatingInvoker(listeners, context);
            return invoker.Task;
        }

        protected virtual void HandleOpenedViewModels(INavigationContext context)
        {
            var viewModelFrom = context.ViewModelFrom;//todo check all presenters
            var viewModelTo = context.ViewModelTo;
            lock (OpenedViewModels)
            {
                if (!OpenedViewModels.TryGetValue(context.NavigationType, out var list))
                {
                    list = new List<WeakNavigationEntry>();
                    OpenedViewModels[context.NavigationType] = list;
                }
                if (viewModelTo != null && (context.NavigationMode == NavigationMode.Refresh || context.NavigationMode == NavigationMode.Back || context.NavigationMode == NavigationMode.New))
                {
                    WeakNavigationEntry? viewModelRef = null;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, viewModelTo))
                        {
                            if (target != null)
                                viewModelRef = list[i];
                            list.RemoveAt(i);
                            --i;
                        }
                    }
                    if (viewModelRef == null)
                        viewModelRef = new WeakNavigationEntry(viewModelTo, context.NavigationProvider, context.NavigationType);
                    list.Add(viewModelRef);
                }
                if (viewModelFrom != null && context.NavigationMode.IsClose())
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var target = list[i].ViewModel;
                        if (target == null || ReferenceEquals(target, viewModelFrom))
                        {
                            list.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }

        protected virtual void OnNavigatedInternal(INavigationContext context)
        {
            var listeners = GetListeners();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnNavigated(context);
        }

        protected virtual void OnNavigationFailedInternal(INavigationContext context, Exception exception)
        {
            var listeners = GetListeners();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnNavigationFailed(context, exception);
        }

        protected virtual void OnNavigationCanceledInternal(INavigationContext context)
        {
            var listeners = GetListeners();
            if (listeners == null)
                return;
            for (var i = 0; i < listeners.Count; i++)
                listeners[i]?.OnNavigationCanceled(context);
        }

        private void GetOpenedViewModelsInternal(NavigationType type, ref List<INavigationEntry>? result)
        {
            if (!OpenedViewModels.TryGetValue(type, out var list))
                return;
            if (result == null)
                result = new List<INavigationEntry>();
            var hasValue = false;
            for (var i = 0; i < list.Count; i++)
            {
                var target = list[i].ToNavigationEntry();
                if (target == null)
                {
                    list.RemoveAt(i);
                    --i;
                }
                else
                {
                    result.Add(target);
                    hasValue = true;
                }
            }

            if (!hasValue)
                OpenedViewModels.Remove(type);
        }

        private void Trace(string navigationName, INavigationContext context)
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceNavigationFormat5, navigationName, context.NavigationMode, context.ViewModelFrom, context.ViewModelTo, context.NavigationType);
        }

        #endregion

        #region Nested types

        protected sealed class NavigatingInvoker : TaskCompletionSource<bool>
        {
            #region Fields

            private readonly INavigationContext _context;
            private readonly INavigationDispatcherListener[] _listeners;
            private int _index;

            #endregion

            #region Constructors

            public NavigatingInvoker(INavigationDispatcherListener[] listeners, INavigationContext context)
            {
                _listeners = listeners;
                _context = context;
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
                        TrySetCanceled();
                        return;
                    }

                    if (!task.Result)
                    {
                        TrySetResult(false);
                        return;
                    }

                    if (_index >= _listeners.Length)
                    {
                        TrySetResult(true);
                        return;
                    }

                    var resultTask = _listeners[_index].OnNavigatingAsync(_context);
                    ++_index;
                    resultTask.ContinueWith(OnExecuted, this, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception e)
                {
                    this.TrySetExceptionEx(e);
                }
            }

            private static void OnExecuted(Task<bool> task, object state)
            {
                ((NavigatingInvoker)state).OnExecuted(task);
            }

            #endregion
        }

        protected sealed class WeakNavigationEntry
        {
            #region Fields

            private readonly WeakReference _providerReference;
            private readonly WeakReference _viewModelReference;

            #endregion

            #region Constructors

            public WeakNavigationEntry(IViewModel viewModel, object? provider, NavigationType navigationType)
            {
                NavigationType = navigationType;
                _viewModelReference = MugenExtensions.GetWeakReference(viewModel);
                _providerReference = MugenExtensions.GetWeakReference(provider);
            }

            #endregion

            #region Properties

            public IViewModel? ViewModel => (IViewModel)_viewModelReference.Target;

            public object? NavigationProvider => _providerReference.Target;

            public NavigationType NavigationType { get; }

            #endregion

            #region Methods

            public INavigationEntry? ToNavigationEntry()
            {
                var viewModel = ViewModel;
                var provider = NavigationProvider;
                if (viewModel == null)
                    return null;
                return new NavigationEntry(NavigationType, viewModel, provider);
            }

            #endregion
        }

        protected sealed class NavigationEntry : INavigationEntry
        {
            #region Constructors

            public NavigationEntry(NavigationType type, IViewModel viewModel, object? provider)
            {
                Should.NotBeNull(type, nameof(type));
                Should.NotBeNull(viewModel, nameof(viewModel));
                Type = type;
                ViewModel = viewModel;
                Provider = provider;
            }

            #endregion

            #region Properties

            public NavigationType Type { get; }

            public IViewModel ViewModel { get; }

            public object? Provider { get; }

            #endregion
        }

        #endregion
    }
}