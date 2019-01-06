using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Presenters.Results;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Fields

        public const int NavigationPresenterPriority = -1;
        public const int MultiViewModelPresenterPriority = 0;
        public const int WindowPresenterPriority = 1;

        private readonly HashSet<NavigationCallback> _pendingOpenCallbacks;
        private readonly PresentersCollection _presenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(INavigationDispatcher navigationDispatcher, ITracer tracer, IMvvmApplication application)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            NavigationDispatcher = navigationDispatcher;
            Tracer = tracer;
            Application = application;
            _presenters = new PresentersCollection(this);
            navigationDispatcher.AddListener(_presenters);
            _pendingOpenCallbacks = new HashSet<NavigationCallback>(ReferenceEqualityComparer.Instance);

            OpenCallbacksKey = GetBuilder<IList<NavigationCallback>?>(nameof(OpenCallbacksKey)).Build();
            CloseCallbacksKey = GetBuilder<IList<NavigationCallback?>?>(nameof(CloseCallbacksKey))
                .SerializableConverter(SerializableConverter)
                .Serializable(CanSerializeCloseCallbacks)
                .Build();
        }

        #endregion

        #region Properties

        public IMetadataContextKey<IList<NavigationCallback>?> OpenCallbacksKey { get; set; }

        public IMetadataContextKey<IList<NavigationCallback?>?> CloseCallbacksKey { get; set; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        protected ITracer Tracer { get; }

        protected IMvvmApplication Application { get; }

        public ICollection<IChildViewModelPresenter> Presenters => _presenters;

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (_presenters.SuspendNavigation())
            {
                var result = ShowInternal(metadata);
                if (result == null)
                    throw ExceptionManager.PresenterCannotShowRequest(metadata.Dump());
                return OnShownInternal(metadata, result);
            }
        }

        public IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryCloseInternal(metadata) ?? ClosingViewModelPresenterResult.FalseResult;
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryRestoreInternal(metadata) ?? RestorationViewModelPresenterResult.Unrestored;
        }

        public Task WaitOpenNavigationAsync(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return WaitOpenNavigationInternalAsync(type, metadata);
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryShow(metadata, this);
                if (operation != null)
                {
                    Trace("show", metadata, presenters[i], operation);
                    return operation;
                }
            }

            return null;
        }

        protected virtual IViewModelPresenterResult OnShownInternal(IReadOnlyMetadataContext metadata, IChildViewModelPresenterResult result)
        {
            var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
            if (viewModel == null)
                throw ExceptionManager.PresenterInvalidRequest(metadata.Dump() + result.Metadata.Dump());

            var openCallback = CreateNavigationCallback(result.NavigationType, false);
            var closeCallback = CreateNavigationCallback(result.NavigationType, Application.IsSerializableCallbacksSupported() && result.IsRestorable);
            AddCallback(viewModel, openCallback, OpenCallbacksKey);
            AddCallback(viewModel, closeCallback, CloseCallbacksKey);

            lock (_pendingOpenCallbacks)
            {
                _pendingOpenCallbacks.Add(openCallback);
            }

            return new ViewModelPresenterResult(result.Metadata, result.NavigationType, openCallback.TaskCompletionSource.Task, closeCallback.TaskCompletionSource.Task);
        }

        protected virtual IClosingViewModelPresenterResult? TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryClose(metadata, this);
                if (operation != null)
                {
                    Trace("close", metadata, presenters[i], operation);
                    return operation;
                }
            }

            var viewModel = metadata.Get(NavigationMetadata.ViewModel);

            if (viewModel != null)
            {
                var closeHandler = viewModel.Metadata.Get(ViewModelMetadata.CloseHandler);
                if (closeHandler != null)
                    return new ClosingViewModelPresenterResult(metadata, closeHandler(NavigationDispatcher, viewModel, metadata));

                //todo fix wrapperviewmodel
                //                var wrapperViewModel = viewModel.Settings.Metadata.GetData(ViewModelConstants.WrapperViewModel);
                //                if (wrapperViewModel != null)
                //                {
                //                    var ctx = new MetadataContext(metadata);
                //                    ctx.Set(NavigationMetadata.ViewModel, wrapperViewModel);
                //                    return TryCloseInternal(metadata);
                //                }
            }

            return null;
        }

        protected virtual IRestorationViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter)
                {
                    var result = presenter.TryRestore(metadata, this);
                    if (result != null)
                    {
                        Trace("restore", metadata, presenter, result);
                        return result;
                    }
                }
            }

            return null;
        }

        protected virtual Task WaitOpenNavigationInternalAsync(NavigationType? type, IReadOnlyMetadataContext metadata)
        {
            List<Task> tasks = null;
            lock (_pendingOpenCallbacks)
            {
                foreach (var pendingOpenCallback in _pendingOpenCallbacks)
                {
                    if (type == null || pendingOpenCallback.NavigationType == type)
                    {
                        if (tasks == null)
                            tasks = new List<Task>();
                        tasks.Add(pendingOpenCallback.TaskCompletionSource.Task);
                    }
                }
            }

            if (tasks == null)
                return Default.CompletedTask;
            return Task.WhenAll(tasks);
        }

        protected virtual void OnChildPresenterAdded(IChildViewModelPresenter presenter)
        {
        }

        protected virtual void OnChildPresenterRemoved(IChildViewModelPresenter presenter)
        {
        }

        protected virtual void OnNavigated(INavigationContext context)
        {
            if (context.NavigationMode.IsClose() && context.ViewModelFrom != null)
            {
                InvokeCallbacks(context.ViewModelFrom, OpenCallbacksKey, context.NavigationType, true, null, false);
                InvokeCallbacks(context.ViewModelFrom, CloseCallbacksKey, context.NavigationType, true, null, false);
            }
            else if (context.ViewModelTo != null)
                InvokeCallbacks(context.ViewModelTo, OpenCallbacksKey, context.NavigationType, true, null, false);
        }

        protected virtual void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            OnNavigationFailed(context, exception, false);
        }

        protected virtual void OnNavigationCanceled(INavigationContext context)
        {
            OnNavigationFailed(context, null, true);
        }

        protected virtual void OnNavigatingCanceled(INavigationContext context)
        {

        }

        protected NavigationCallback CreateNavigationCallback(NavigationType type, bool serializable)
        {
            return new NavigationCallback(type, serializable);
        }

        private void OnNavigationFailed(INavigationContext context, Exception e, bool canceled)
        {
            if (context.NavigationMode.IsClose())
            {
                if (context.ViewModelFrom != null)
                {
                    InvokeCallbacks(context.ViewModelFrom, OpenCallbacksKey, context.NavigationType, false, e, canceled);
                    InvokeCallbacks(context.ViewModelFrom, CloseCallbacksKey, context.NavigationType, false, e, canceled);
                }
            }
            else if (context.ViewModelTo != null)
            {
                InvokeCallbacks(context.ViewModelTo, OpenCallbacksKey, context.NavigationType, false, e, canceled);
                InvokeCallbacks(context.ViewModelTo, CloseCallbacksKey, context.NavigationType, false, e, canceled);
            }
        }

        private static void AddCallback(IViewModel viewModel, NavigationCallback callback, IMetadataContextKey<IList<NavigationCallback>?> key)
        {
            var callbacks = viewModel.Metadata.GetOrAdd(key, (object)null, (object)null, (context, o, arg3) => new List<NavigationCallback>());
            lock (callback)
            {
                callbacks.Add(callback);
            }
        }

        private void InvokeCallbacks(IViewModel viewModel, IMetadataContextKey<IList<NavigationCallback>?> key, NavigationType navigationType, bool result, Exception exception, bool canceled)
        {
            var callbacks = viewModel.Metadata.Get(key);
            if (callbacks == null)
                return;
            List<NavigationCallback> toInvoke = null;
            lock (callbacks)
            {
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var callback = callbacks[i];
                    if (callback == null || callback.NavigationType == navigationType)
                    {
                        if (callback != null)
                        {
                            if (toInvoke == null)
                                toInvoke = new List<NavigationCallback>();
                            toInvoke.Add(callback);
                        }

                        callbacks.RemoveAt(i);
                        --i;
                    }
                }
            }

            if (toInvoke == null)
                return;
            for (var i = 0; i < toInvoke.Count; i++)
            {
                var callback = toInvoke[i];
                callback.SetResult(result, exception, canceled);
                lock (_pendingOpenCallbacks)
                {
                    _pendingOpenCallbacks.Remove(callback);
                }
            }
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name) where T : class
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelPresenter), name).NotNull();
        }

        private static object SerializableConverter(IMetadataContextKey<IList<NavigationCallback?>?> arg1, object? value, ISerializationContext arg3)
        {
            var callbacks = (IList<NavigationCallback>)value;
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                return callbacks.Where(callback => callback.Serializable && !callback.TaskCompletionSource.Task.IsCompleted).ToList();
            }
        }

        private static bool CanSerializeCloseCallbacks(IMetadataContextKey<IList<NavigationCallback>> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallback>)value;
            return callbacks != null && callbacks.Any(callback => callback != null && callback.Serializable);
        }

        private void Trace(string requestName, IReadOnlyMetadataContext metadata, IChildViewModelPresenter presenter, IHasMetadata<IReadOnlyMetadataContext> hasMetadata)
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceViewModelPresenterFormat3, requestName, metadata.Dump() + hasMetadata.Metadata.Dump(), presenter.GetType().FullName);
        }

        #endregion

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class NavigationCallback
        {
            #region Fields

            [DataMember(Name = "N")]
            public readonly NavigationType NavigationType;

            [DataMember(Name = "N")]
            public readonly bool Serializable;

            [DataMember(Name = "T")]
            public readonly TaskCompletionSource<bool> TaskCompletionSource;

            #endregion

            #region Constructors

            internal NavigationCallback(NavigationType navigationType, bool serializable)
            {
                TaskCompletionSource = new TaskCompletionSource<bool>();
                NavigationType = navigationType;
                Serializable = serializable;
            }

            internal NavigationCallback()
            {
            }

            #endregion

            #region Methods

            public void SetResult(bool result, Exception exception, bool canceled)
            {
                if (exception != null)
                    TaskCompletionSource.TrySetExceptionEx(exception);
                else if (canceled)
                    TaskCompletionSource.TrySetCanceled();
                else
                    TaskCompletionSource.TrySetResult(result);
            }

            #endregion
        }

        private sealed class PresentersCollection : ICollection<IChildViewModelPresenter>, IComparer<IChildViewModelPresenter>, INavigationDispatcherListener, IDisposable
        {
            #region Fields

            private readonly OrderedListInternal<IChildViewModelPresenter> _list;
            private readonly ViewModelPresenter _presenter;
            private readonly List<KeyValuePair<INavigationContext, object>> _suspendedEvents;
            private int _suspendCount;

            #endregion

            #region Constructors

            public PresentersCollection(ViewModelPresenter presenter)
            {
                _presenter = presenter;
                _list = new OrderedListInternal<IChildViewModelPresenter>(this);
                _suspendedEvents = new List<KeyValuePair<INavigationContext, object>>();
            }

            #endregion

            #region Properties

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            #endregion

            #region Implementation of interfaces

            public IEnumerator<IChildViewModelPresenter> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IChildViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                _list.Add(item);
                _presenter.OnChildPresenterAdded(item);
            }

            public void Clear()
            {
                var values = _list.ToArray();
                _list.Clear();
                for (var index = 0; index < values.Length; index++)
                    _presenter.OnChildPresenterRemoved(values[index]);
            }

            public bool Contains(IChildViewModelPresenter item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(IChildViewModelPresenter[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(IChildViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                var remove = _list.Remove(item);
                if (remove)
                    _presenter.OnChildPresenterRemoved(item);
                return remove;
            }

            int IComparer<IChildViewModelPresenter>.Compare(IChildViewModelPresenter x1, IChildViewModelPresenter x2)
            {
                return x2.Priority.CompareTo(x1.Priority);
            }

            public void Dispose()
            {
                KeyValuePair<INavigationContext, object>[] events;
                lock (_suspendedEvents)
                {
                    if (--_suspendCount != 0)
                        return;
                    events = _suspendedEvents.ToArray();
                    _suspendedEvents.Clear();
                }

                for (var i = 0; i < events.Length; i++)
                {
                    var pair = events[i];
                    if (pair.Value == null)
                        OnNavigated(pair.Key);
                    else if (pair.Value is Exception e)
                        OnNavigationFailed(pair.Key, e);
                    else
                        OnNavigationCanceled(pair.Key);
                }
            }

            Task<bool> INavigationDispatcherListener.OnNavigatingAsync(INavigationContext context)
            {
                return Default.TrueTask;
            }

            public void OnNavigated(INavigationContext context)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, null));
                        return;
                    }
                }

                _presenter.OnNavigated(context);
            }

            public void OnNavigationFailed(INavigationContext context, Exception exception)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, exception));
                        return;
                    }
                }

                _presenter.OnNavigationFailed(context, exception);
            }

            public void OnNavigationCanceled(INavigationContext context)
            {
                lock (_suspendedEvents)
                {
                    if (_suspendCount != 0)
                    {
                        _suspendedEvents.Add(new KeyValuePair<INavigationContext, object>(context, context));
                        return;
                    }
                }

                _presenter.OnNavigationCanceled(context);
            }

            public void OnNavigatingCanceled(INavigationContext context)
            {
                _presenter.OnNavigatingCanceled(context);
            }

            #endregion

            #region Methods

            public IDisposable SuspendNavigation()
            {
                lock (_suspendedEvents)
                {
                    ++_suspendCount;
                }

                return this;
            }

            #endregion
        }

        #endregion
    }
}