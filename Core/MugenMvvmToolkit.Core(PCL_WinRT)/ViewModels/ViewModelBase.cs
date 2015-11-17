#region Copyright

// ****************************************************************************
// <copyright file="ViewModelBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 9)]
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IIocContainerOwnerViewModel, IHandler<object>
    {
        #region Nested types

        private sealed class BusyToken : IBusyToken, IBusyTokenCallback
        {
            #region Fields

            private static readonly List<IBusyTokenCallback> CompletedList;

            private readonly WeakReference _ref;
            private readonly object _message;

            private List<IBusyTokenCallback> _listeners;
            private BusyToken _prev;
            private BusyToken _next;

            #endregion

            #region Constructors

            static BusyToken()
            {
                CompletedList = new List<IBusyTokenCallback>(1);
            }

            public BusyToken(IHasWeakReference vm, object message)
            {
                _message = message;
                _ref = vm.WeakReference;
            }

            public BusyToken(IHasWeakReference vm, IBusyToken token)
                : this(vm, token.Message)
            {
                token.Register(this);
            }

            #endregion

            #region Methods

            public bool Combine(ViewModelBase vm)
            {
                if (IsCompleted)
                    return false;
                object oldMessage;
                bool oldBusy;
                lock (_ref)
                {
                    if (IsCompleted)
                        return false;
                    if (vm._busyTail != null)
                    {
                        _prev = vm._busyTail;
                        vm._busyTail._next = this;
                    }
                    oldMessage = vm.BusyMessage;
                    oldBusy = vm.IsBusy;
                    vm._busyTail = this;
                }
                if (oldMessage != vm.BusyMessage)
                    vm.OnPropertyChanged(Empty.BusyMessageChangedArgs);
                if (oldBusy != vm.IsBusy)
                    vm.OnPropertyChanged(Empty.IsBusyChangedArgs);
                return true;
            }

            public IList<IBusyToken> GetTokens(ViewModelBase vm)
            {
                IList<IBusyToken> tokens = null;
                lock (_ref)
                {
                    var token = vm._busyTail;
                    while (token != null)
                    {
                        if (tokens == null)
                            tokens = new List<IBusyToken>();
                        tokens.Insert(0, token);
                        token = token._prev;
                    }
                }
                return tokens ?? Empty.Array<IBusyToken>();
            }

            #endregion

            #region Implementation of IBusyToken

            public bool IsCompleted
            {
                get { return ReferenceEquals(CompletedList, _listeners); }
            }

            public object Message
            {
                get { return _message; }
            }

            public void Register(IBusyTokenCallback callback)
            {
                if (IsCompleted)
                {
                    callback.OnCompleted(this);
                    return;
                }
                lock (_ref)
                {
                    if (!IsCompleted)
                    {
                        if (_listeners == null)
                            _listeners = new List<IBusyTokenCallback>(2);
                        _listeners.Add(callback);
                        return;
                    }
                }
                callback.OnCompleted(this);
            }

            public void Dispose()
            {
                IBusyTokenCallback[] listeners = null;
                ViewModelBase vm = null;
                object oldMessage = null;
                bool oldBusy = false;
                lock (_ref)
                {
                    if (_listeners != null)
                        listeners = _listeners.ToArray();
                    if (_prev != null)
                        _prev._next = _next;
                    if (_next == null)
                    {
                        vm = _ref.Target as ViewModelBase;
                        if (vm != null)
                        {
                            oldMessage = vm.BusyMessage;
                            oldBusy = vm.IsBusy;
                            vm._busyTail = _prev;
                        }
                    }
                    else
                        _next._prev = _prev;
                    _listeners = CompletedList;

                }
                if (listeners != null)
                {
                    for (int i = 0; i < listeners.Length; i++)
                        listeners[i].OnCompleted(this);
                }
                if (vm != null)
                {
                    if (oldBusy != vm.IsBusy)
                        vm.OnPropertyChanged(Empty.IsBusyChangedArgs);
                    if (oldMessage != vm.BusyMessage)
                        vm.OnPropertyChanged(Empty.BusyMessageChangedArgs);
                }
            }

            public void OnCompleted(IBusyToken token)
            {
                Dispose();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly CancellationTokenSource DisposedToken;

        private const int DisposingRestoredState = DisposingState | RestoredState;
        private const int DisposingState = 4;
        private const int RestoredState = 2;
        private const int InitializedState = 1;
        private const int DefaultState = 0;

        private readonly IViewModelSettings _settings;

        private BusyToken _busyTail;
        private CancellationTokenSource _disposeCancellationToken;
        private IIocContainer _iocContainer;
        private IEventAggregator _localEventAggregator;
        private int _state;

        #endregion

        #region Constructors

        static ViewModelBase()
        {
            DisposedToken = new CancellationTokenSource();
            DisposedToken.Cancel();
        }

        protected ViewModelBase()
        {
            var current = MvvmApplication.Current;
            if (current == null || current.ViewModelSettings == null)
                _settings = new DefaultViewModelSettings();
            else
                _settings = current.ViewModelSettings.Clone();
            Tracer.TraceViewModel(AuditAction.Created, this);
            if (IsDesignMode)
                ServiceProvider.DesignTimeManager.InitializeViewModel(this);
        }

        #endregion

        #region Properties

        protected static bool IsDesignMode
        {
            get { return ServiceProvider.DesignTimeManager.IsDesignMode; }
        }

        protected internal IEventAggregator LocalEventAggregator
        {
            get
            {
                InitializeEventAggregator(true);
                return _localEventAggregator;
            }
        }

        protected internal bool IsRestored
        {
            get { return (_state & RestoredState) == RestoredState; }
        }

        protected virtual IViewModelProvider ViewModelProvider
        {
            get { return ServiceProvider.ViewModelProvider; }
        }

        #endregion

        #region Implementation of IViewModel

        public CancellationToken DisposeCancellationToken
        {
            get
            {
                if (_disposeCancellationToken == null)
                {
                    var cts = new CancellationTokenSource();
                    Interlocked.CompareExchange(ref _disposeCancellationToken, cts, null);
                    if (!ReferenceEquals(cts, _disposeCancellationToken))
                        cts.Dispose();
                }
                return _disposeCancellationToken.Token;
            }
        }

        public bool IsInitialized
        {
            get { return _state != DefaultState; }
        }

        public virtual bool IsBusy
        {
            get { return _busyTail != null; }
        }

        public virtual object BusyMessage
        {
            get
            {
                var tail = _busyTail;
                if (tail == null)
                    return null;
                return tail.Message;
            }
        }

        public IViewModelSettings Settings
        {
            get { return _settings; }
        }

        public virtual IIocContainer IocContainer
        {
            get
            {
                if (_iocContainer != null)
                    return _iocContainer;
                var viewModel = this.GetParentViewModel();
                if (viewModel == null)
                    return ServiceProvider.IocContainer;
                return viewModel.GetIocContainer(true, false);
            }
            protected internal set
            {
                if (Equals(_iocContainer, value))
                    return;
                _iocContainer = value;
                OnPropertyChanged("IocContainer");
            }
        }

        public bool IsDisposed
        {
            get { return _disposeCancellationToken != null && _disposeCancellationToken.IsCancellationRequested; }
        }

        void IViewModel.InitializeViewModel(IDataContext context)
        {
            EnsureNotDisposed();
            Should.NotBeNull(context, "context");
            if (Interlocked.CompareExchange(ref _state, InitializedState, DefaultState) != DefaultState)
            {
                Tracer.Warn(ExceptionManager.ObjectInitialized("ViewModel", this).Message);
                return;
            }
            bool restored;
            context.TryGetData(InitializationConstants.IsRestored, out restored);
            if (restored)
                Interlocked.CompareExchange(ref _state, RestoredState, InitializedState);
            IocContainer = context.GetData(InitializationConstants.IocContainer);

            OnInitializing(context);
            OnInitializedInternal();
            OnInitialized();
            var handler = Initialized;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                Initialized = null;
            }
            OnPropertyChanged("IsInitialized");
            Tracer.TraceViewModel(AuditAction.Initialized, this);
        }

        public virtual IBusyToken BeginBusy(object message = null)
        {
            var token = new BusyToken(this, message ?? Settings.DefaultBusyMessage);
            token.Combine(this);
            Publish(token);
            OnBeginBusy(token);
            return token;
        }

        public virtual IList<IBusyToken> GetBusyTokens()
        {
            var tail = _busyTail;
            if (tail == null)
                return Empty.Array<IBusyToken>();
            return tail.GetTokens(this);
        }

        public void Publish(object sender, object message)
        {
            if (ReferenceEquals(sender, this))
                PublishInternal(sender, message);
            else
            {
                EventAggregator.Publish(this, sender, message);
                if (!Settings.BroadcastAllMessages && !(message is IBroadcastMessage))
                    PublishInternal(sender, message);
            }
        }

        public bool Subscribe(ISubscriber subscriber)
        {
            if (!SubscribeInternal(subscriber))
                return false;
            var vm = subscriber.Target as IViewModel;
            if (vm != null)
            {
                var tokens = GetBusyTokens();
                if (vm is ViewModelBase)
                {
                    for (int i = 0; i < tokens.Count; i++)
                        ((IHandler<object>)vm).Handle(this, tokens[i]);
                }
                else
                {
                    for (int i = 0; i < tokens.Count; i++)
                        vm.Publish(this, tokens[i]);
                }
            }
            return true;
        }

        public bool Unsubscribe(ISubscriber subscriber)
        {
            return UnsubscribeInternal(subscriber);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposingState, InitializedState) != InitializedState &&
                Interlocked.CompareExchange(ref _state, DisposingState, DefaultState) != DefaultState &&
                Interlocked.CompareExchange(ref _state, DisposingRestoredState, RestoredState) != RestoredState)
                return;
            try
            {
                GC.SuppressFinalize(this);
                if (Settings.DisposeCommands)
                    ReflectionExtensions.DisposeCommands(this);
                OnDisposeInternal(true);
                OnDispose(true);
                var handler = Disposed;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                    Disposed = null;
                }
                DisposeInternal();
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _disposeCancellationToken, DisposedToken, null) != null)
                    _disposeCancellationToken.Cancel();
            }
        }

        public event EventHandler<IViewModel, EventArgs> Initialized;

        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        void IHandler<object>.Handle(object sender, object message)
        {
            if (!ReferenceEquals(sender, this))
                HandleInternal(sender, message);
            OnHandle(sender, message);
        }

        void IIocContainerOwnerViewModel.RequestOwnIocContainer()
        {
            OnRequestOwnIocContainer();
        }

        #endregion

        #region Methods

        protected internal IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel, ObservationMode? observationMode = null,
            params DataConstantValue[] parameters)
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(getViewModel, this, observationMode, parameters);
        }

        protected internal T GetViewModel<T>([NotNull] GetViewModelDelegate<T> getViewModelGeneric, ObservationMode? observationMode = null,
            params DataConstantValue[] parameters) where T : class, IViewModel
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(getViewModelGeneric, this, observationMode, parameters);
        }

        protected internal IViewModel GetViewModel([NotNull] Type viewModelType, ObservationMode? observationMode = null, params DataConstantValue[] parameters)
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(viewModelType, this, observationMode, parameters);
        }

        protected internal T GetViewModel<T>(ObservationMode? observationMode = null, params DataConstantValue[] parameters) where T : IViewModel
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel<T>(this, observationMode, parameters);
        }

        protected void Publish([NotNull] object message, ExecutionMode mode = ExecutionMode.None)
        {
            ThreadManager.Invoke(mode, this, message, (@base, o) => @base.Publish(@base, o));
        }

        protected void EnsureNotDisposed()
        {
            this.NotBeDisposed();
        }

        internal virtual void HandleInternal(object sender, object message)
        {
            var busyToken = message as IBusyToken;
            if (busyToken != null)
            {
                HandleMode messageMode = Settings.HandleBusyMessageMode;
                if (messageMode.HasFlagEx(HandleMode.Handle))
                {
                    var token = new BusyToken(this, busyToken);
                    if (token.Combine(this))
                        OnBeginBusy(token);
                }
                if (messageMode.HasFlagEx(HandleMode.NotifySubscribers))
                    PublishInternal(sender, message);
                return;
            }
            if (Settings.BroadcastAllMessages || message is IBroadcastMessage)
                PublishInternal(sender, message);
        }

        private void DisposeInternal()
        {
            this.ClearBusy();
            ClearPropertyChangedSubscribers();
            if (InitializeEventAggregator(false))
            {
                var toRemove = _localEventAggregator.GetSubscribers();
                for (int index = 0; index < toRemove.Count; index++)
                    Unsubscribe(toRemove[index]);
            }

            IViewManager viewManager;
            if (IocContainer.TryGet(out viewManager))
                viewManager.CleanupViewAsync(this);

            Settings.Metadata.Clear();
            ServiceProvider.AttachedValueProvider.Clear(this);
            Tracer.TraceViewModel(AuditAction.Disposed, this);
        }

        private bool InitializeEventAggregator(bool required)
        {
            if (_localEventAggregator != null)
                return true;
            if (!required)
                return false;
            Interlocked.CompareExchange(ref _localEventAggregator, ServiceProvider.InstanceEventAggregatorFactory(this), null);
            return true;
        }

        #endregion

        #region Overrides of NotifyPropertyChangedBase

        internal override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            Publish(StateChangedMessage.Empty);
        }

        #endregion

        #region Virtual methods

        protected virtual bool SubscribeInternal(ISubscriber subscriber)
        {
            return !ReferenceEquals(subscriber.Target, this) && InitializeEventAggregator(true) &&
                   _localEventAggregator.Subscribe(subscriber);
        }

        protected virtual bool UnsubscribeInternal(ISubscriber subscriber)
        {
            if (InitializeEventAggregator(false))
                return _localEventAggregator.Unsubscribe(subscriber);
            return false;
        }

        protected virtual void PublishInternal(object sender, object message)
        {
            if (InitializeEventAggregator(false))
                _localEventAggregator.Publish(sender, message);
        }

        protected virtual void OnRequestOwnIocContainer()
        {
            if (_iocContainer == null)
                _iocContainer = IocContainer.CreateChild();
        }

        protected virtual void OnHandle(object sender, object message)
        {
        }

        protected virtual void OnBeginBusy(IBusyToken token)
        {
        }

        internal virtual void OnInitializedInternal()
        {
        }

        protected virtual void OnInitializing(IDataContext context)
        {
        }

        protected virtual void OnInitialized()
        {
        }

        internal virtual void OnDisposeInternal(bool disposing)
        {
        }

        protected virtual void OnDispose(bool disposing)
        {
        }

        #endregion

        #region Destructor

        ~ViewModelBase()
        {
            OnDisposeInternal(false);
            OnDispose(false);
            Tracer.TraceViewModel(AuditAction.Finalized, this);
        }

        #endregion
    }
}
