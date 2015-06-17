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
    /// <summary>
    ///     Represents the base class for all view models.
    /// </summary>
    [BaseViewModel(Priority = 9)]
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModel, IHandler<object>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelBase" /> class.
        /// </summary>
        protected ViewModelBase()
        {
            _settings = ApplicationSettings.ViewModelSettings.Clone();
            Tracer.TraceViewModel(AuditAction.Created, this);
            if (IsDesignMode)
                ServiceProvider.DesignTimeManager.InitializeViewModel(this);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value indicating whether the control is in design mode (running under Blend or Visual Studio).
        /// </summary>
        protected static bool IsDesignMode
        {
            get { return ServiceProvider.DesignTimeManager.IsDesignMode; }
        }

        /// <summary>
        ///     Gets the current <see cref="IEventAggregator" />.
        /// </summary>
        protected internal IEventAggregator LocalEventAggregator
        {
            get
            {
                InitializeEventAggregator(true);
                return _localEventAggregator;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is restored.
        /// </summary>
        protected internal bool IsRestored
        {
            get { return (_state & RestoredState) == RestoredState; }
        }

        /// <summary>
        ///     Gets or sets the default view model provider to create view models.
        /// </summary>
        protected virtual IViewModelProvider ViewModelProvider
        {
            get { return ServiceProvider.ViewModelProvider; }
        }

        #endregion

        #region Implementation of IViewModel

        /// <summary>
        ///     Gets the cancellation token that uses to cancel an operation when the current view model will be disposed.
        /// </summary>
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

        /// <summary>
        ///     Gets a value indicating whether this view model is initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return _state != DefaultState; }
        }

        /// <summary>
        ///     Gets the busy state of the current view model.
        /// </summary>
        public virtual bool IsBusy
        {
            get { return _busyTail != null; }
        }

        /// <summary>
        ///     Gets the information message for busy dialog.
        /// </summary>
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

        /// <summary>
        ///     Gets the <see cref="IViewModelSettings" />.
        /// </summary>
        public IViewModelSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        ///     Gets the ioc adapter of the current view model.
        /// </summary>
        public IIocContainer IocContainer
        {
            get
            {
                if (_iocContainer == null)
                    return ServiceProvider.IocContainer;
                return _iocContainer;
            }
            internal set
            {
                if (ReferenceEquals(_iocContainer, value))
                    return;
                _iocContainer = value;
                OnPropertyChanged("IocContainer");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposeCancellationToken != null && _disposeCancellationToken.IsCancellationRequested; }
        }

        /// <summary>
        ///     Initializes the current view model using the specified <see cref="IDataContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
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
            IocContainer = context.GetData(InitializationConstants.IocContainer, true);

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

        /// <summary>
        ///     Begins to indicate that the current view model is busy.
        /// </summary>
        /// <param name="message">
        ///     The specified message for the <see cref="IViewModel.BusyMessage" /> property.
        /// </param>
        /// <returns>Id of the operation.</returns>
        public virtual IBusyToken BeginBusy(object message = null)
        {
            var token = new BusyToken(this, message ?? Settings.DefaultBusyMessage);
            token.Combine(this);
            Publish(token);
            OnBeginBusy(token);
            return token;
        }

        /// <summary>
        ///     Gets the collection of busy tokens.
        /// </summary>
        public virtual IList<IBusyToken> GetBusyTokens()
        {
            var tail = _busyTail;
            if (tail == null)
                return Empty.Array<IBusyToken>();
            return tail.GetTokens(this);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">The message instance.</param>
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

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
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

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        public bool Unsubscribe(ISubscriber subscriber)
        {
            return UnsubscribeInternal(subscriber);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposingState, InitializedState) != InitializedState &&
                Interlocked.CompareExchange(ref _state, DisposingState, DefaultState) != DefaultState &&
                Interlocked.CompareExchange(ref _state, DisposingRestoredState, RestoredState) != RestoredState)
                return;
            try
            {
                GC.SuppressFinalize(this);
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

        /// <summary>
        ///     Occurs when this <see cref="IViewModel" /> is initialized.
        ///     This event coincides with cases where the value of the <see cref="IsInitialized" /> property changes from false to true.
        /// </summary>
        public event EventHandler<IViewModel, EventArgs> Initialized;

        /// <summary>
        ///     Occurs when the object is disposed by a call to the Dispose method.
        /// </summary>
        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IHandler<object>.Handle(object sender, object message)
        {
            if (!ReferenceEquals(sender, this))
                HandleInternal(sender, message);
            OnHandle(sender, message);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal IViewModel GetViewModel([NotNull] GetViewModelDelegate<IViewModel> getViewModel, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(getViewModel, this, observationMode, containerCreationMode, parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModelGeneric">The specified delegate to create view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal T GetViewModel<T>([NotNull] GetViewModelDelegate<T> getViewModelGeneric, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters) where T : class, IViewModel
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(getViewModelGeneric, this, observationMode, containerCreationMode, parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal IViewModel GetViewModel([NotNull] Type viewModelType, ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters)
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel(viewModelType, this, observationMode, containerCreationMode, parameters);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <typeparam name="T">The type of view model.</typeparam>
        /// <param name="containerCreationMode">The value that is responsible to initialize the IocContainer.</param>
        /// <param name="observationMode">The value that is responsible for listen messages in created view model.</param>
        /// <param name="parameters">The specified parameters to get view-model.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        protected internal T GetViewModel<T>(ObservationMode? observationMode = null,
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters) where T : IViewModel
        {
            EnsureNotDisposed();
            return ViewModelProvider.GetViewModel<T>(this, observationMode, containerCreationMode, parameters);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        /// <param name="mode">The execution mode.</param>
        protected void Publish([NotNull] object message, ExecutionMode mode = ExecutionMode.None)
        {
            ThreadManager.Invoke(mode, this, message, (@base, o) => @base.Publish(@base, o));
        }

        /// <summary>
        ///     Makes sure that the object is not disposed.
        /// </summary>
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
            //Disposing commands, if need.
            if (Settings.DisposeCommands)
                ReflectionExtensions.DisposeCommands(this);

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

            //Disposing ioc adapter.
            if (Settings.DisposeIocContainer && _iocContainer != null)
                _iocContainer.Dispose();

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

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
        protected virtual bool SubscribeInternal(ISubscriber subscriber)
        {
            return !ReferenceEquals(subscriber.Target, this) && InitializeEventAggregator(true) &&
                   _localEventAggregator.Subscribe(subscriber);
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        protected virtual bool UnsubscribeInternal(ISubscriber subscriber)
        {
            if (InitializeEventAggregator(false))
                return _localEventAggregator.Unsubscribe(subscriber);
            return false;
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The specified sender.</param>
        /// <param name="message">The message instance.</param>
        protected virtual void PublishInternal(object sender, object message)
        {
            if (InitializeEventAggregator(false))
                _localEventAggregator.Publish(sender, message);
        }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        protected virtual void OnHandle(object sender, object message)
        {
        }

        /// <summary>
        ///     This method will be invoked when a busy operation is started.
        /// </summary>
        protected virtual void OnBeginBusy(IBusyToken token)
        {
        }

        /// <summary>
        ///     Occurs after the view model is fully loaded.
        /// </summary>
        internal virtual void OnInitializedInternal()
        {
        }

        /// <summary>
        ///     Occurs during the initialization of the view model.
        /// </summary>
        protected virtual void OnInitializing(IDataContext context)
        {
        }

        /// <summary>
        ///     Occurs after the view model is fully loaded.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        ///     Occurs after the current view model is disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal virtual void OnDisposeInternal(bool disposing)
        {
        }

        /// <summary>
        ///     Occurs after the current view model is disposed, use for clear resource and event listeners.
        /// </summary>
        protected virtual void OnDispose(bool disposing)
        {
        }

        #endregion

        #region Destructor

        /// <summary>
        ///     Destructor of view model.
        /// </summary>
        ~ViewModelBase()
        {
            OnDisposeInternal(false);
            OnDispose(false);
            Tracer.TraceViewModel(AuditAction.Finalized, this);
        }

        #endregion
    }
}