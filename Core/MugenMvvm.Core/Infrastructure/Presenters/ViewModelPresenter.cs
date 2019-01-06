using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Fields

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

            OpenCallbacksKey = GetBuilder<IList<NavigationCallback>?>(nameof(OpenCallbacksKey))
                .NotNull()
                .Build();
            CloseCallbacksKey = GetBuilder<IList<NavigationCallback?>?>(nameof(CloseCallbacksKey))
                .NotNull()
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
                var result = ShowInternalAsync(metadata);
                if (result == null)
                    throw ExceptionManager.PresenterCannotShowRequest(metadata.Dump());

                var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                if (viewModel != null)
                {
                    var openCallback = CreateNavigationCallback(result.NavigationType, false);
                    var closeCallback = CreateNavigationCallback(result.NavigationType, Application.IsSerializableCallbacksSupported() && result.IsRestorable);
                }

                return null;
            }
        }

        public IClosingViewModelPresenterResult TryClose(IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternalAsync(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.ToArray();
            for (var i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryShowAsync(metadata, this);
                if (operation != null)
                {
                    Trace("show", metadata, presenters[i]);
                    return operation;
                }
            }

            return null;
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
            }
            else if (context.ViewModelTo != null)
            {
            }
        }

        protected virtual void OnNavigationFailed(INavigationContext context, Exception exception)
        {
            var viewModel = context.NavigationMode.IsCloseOrBackground() ? context.ViewModelFrom : context.ViewModelTo;
            if (viewModel != null)
            {
            }
        }

        protected virtual void OnNavigationCanceled(INavigationContext context)
        {
            var viewModel = context.NavigationMode.IsCloseOrBackground() ? context.ViewModelFrom : context.ViewModelTo;
            if (viewModel != null)
            {
            }
        }

        protected NavigationCallback CreateNavigationCallback(NavigationType type, bool serializable)
        {
            return new NavigationCallback(new TaskCompletionSource<object>(), type, serializable);
        }

        private void Trace(string requestName, IReadOnlyMetadataContext metadata, IChildViewModelPresenter presenter)
        {
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Info(MessageConstants.TraceViewModelPresenterFormat3, requestName, metadata.Dump(), presenter.GetType().FullName);
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name) where T : class
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelPresenter), name).NotNull();
        }

        private static bool CanSerializeCloseCallbacks(IMetadataContextKey<IList<NavigationCallback>> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<NavigationCallback>)value;
            return callbacks != null && callbacks.Any(callback => callback != null && callback.Serializable);
        }

        #endregion

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class NavigationCallback : IHasMemento, IMemento
        {
            #region Fields

            [DataMember(Name = "N")]
            public readonly NavigationType NavigationType;

            [DataMember(Name = "N")]
            public readonly bool Serializable;

            [DataMember(Name = "T")]
            public readonly TaskCompletionSource<object> TaskCompletionSource;

            #endregion

            #region Constructors

            internal NavigationCallback(TaskCompletionSource<object> taskCompletionSource, NavigationType navigationType, bool serializable)
            {
                TaskCompletionSource = taskCompletionSource;
                NavigationType = navigationType;
                Serializable = serializable;
            }

            internal NavigationCallback()
            {
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(NavigationCallback);

            #endregion

            #region Implementation of interfaces

            public IMemento? GetMemento()
            {
                if (Serializable)
                    return this;
                return null;
            }

            void IMemento.Preserve(ISerializationContext serializationContext)
            {
            }

            IMementoResult IMemento.Restore(ISerializationContext serializationContext)
            {
                return new MementoResult(this, serializationContext);
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