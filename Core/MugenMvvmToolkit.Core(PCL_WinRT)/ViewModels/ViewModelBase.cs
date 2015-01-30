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
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.DataConstants;
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

        private sealed class BusyDict : LightDictionaryBase<Guid, object>
        {
            #region Constructors

            public BusyDict()
                : base(false)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<Guid,object>

            protected override bool Equals(Guid x, Guid y)
            {
                return x == y;
            }

            protected override int GetHashCode(Guid key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly CancellationTokenSource DisposedToken;
        private const int DisposedState = 2;
        private const int InitializedState = 1;
        private const int DefaultState = 0;

        private readonly IViewModelSettings _settings;

        private CancellationTokenSource _disposeCancellationToken;
        private IIocContainer _iocContainer;
        private IEventAggregator _localEventAggregator;
        private BusyDict _busyMessages;
        private object _busyMessage;
        private int _state;
        private bool _isBusy;
        private bool _isRestored;

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
        protected bool IsRestored
        {
            get { return _isRestored; }
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
        ///     Gets the initialized state of the current view model.
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
            get { return _isBusy; }
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        /// <summary>
        ///     Gets the information message for busy dialog.
        /// </summary>
        public virtual object BusyMessage
        {
            get { return _busyMessage; }
            private set
            {
                if (_busyMessage == value) return;
                _busyMessage = value;
                OnPropertyChanged("BusyMessage");
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
            context.TryGetData(InitializationConstants.IsRestored, out _isRestored);
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
        public Guid BeginBusy(object message = null)
        {
            Guid newGuid = Guid.NewGuid();
            if (message == null)
                message = Settings.DefaultBusyMessage;
            AddBusy(newGuid, message, true);
            return newGuid;
        }

        /// <summary>
        ///     Ends to indicate that the current view model is busy.
        /// </summary>
        /// <param name="idBusy">Id of the operation to end.</param>
        public void EndBusy(Guid idBusy)
        {
            RemoveBusy(idBusy, true);
        }

        /// <summary>
        ///     Clears all busy operations.
        /// </summary>
        public void ClearBusy()
        {
            if (!InitializeBusyDict(false))
                return;
            KeyValuePair<Guid, object>[] messages;
            lock (_busyMessages)
                messages = _busyMessages.ToArray();
            for (int index = 0; index < messages.Length; index++)
                EndBusy(messages[index].Key);
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
                //NOTE calling all handlers in this view model.
                HandlerSubscriber.GetOrCreate(this).Handle(sender, message);
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
                NotifyBeginBusy(vm);
            return true;
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        public bool Unsubscribe(ISubscriber subscriber)
        {
            if (!UnsubscribeInternal(subscriber))
                return false;
            var vm = subscriber.Target as IViewModel;
            if (vm != null)
                NotifyEndBusy(vm);
            return true;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
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
            IocContainerCreationMode? containerCreationMode = null, params DataConstantValue[] parameters) where T : IViewModel
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
        protected internal IViewModel GetViewModel([NotNull, ViewModelTypeRequired] Type viewModelType, ObservationMode? observationMode = null,
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
            var beginBusyMessage = message as BeginBusyMessage;
            if (beginBusyMessage != null)
            {
                HandleMode messageMode = Settings.HandleBusyMessageMode;
                if (messageMode.HasFlagEx(HandleMode.Handle))
                    AddBusy(beginBusyMessage.Id, beginBusyMessage.Message, false);
                if (messageMode.HasFlagEx(HandleMode.NotifySubscribers))
                    PublishInternal(sender, message);
                return;
            }
            var endBusyMessage = message as EndBusyMessage;
            if (endBusyMessage != null)
            {
                RemoveBusy(endBusyMessage.Id, false);
                PublishInternal(sender, message);
                return;
            }
            if (Settings.BroadcastAllMessages || message is IBroadcastMessage)
                PublishInternal(sender, message);
        }

        private void NotifyBeginBusy(IViewModel viewModel)
        {
            if (!InitializeBusyDict(false))
                return;
            lock (_busyMessages)
            {
                if (_busyMessages.Count == 0)
                    return;
                var isVmb = viewModel is ViewModelBase;
                foreach (var o in _busyMessages)
                {
                    var message = new BeginBusyMessage(o.Key, o.Value);
                    if (isVmb)
                        ((IHandler<object>)viewModel).Handle(this, message);
                    else
                        viewModel.Publish(this, message);
                }
            }
        }

        private void NotifyEndBusy(IViewModel viewModel)
        {
            if (!InitializeBusyDict(false))
                return;
            lock (_busyMessages)
            {
                if (_busyMessages.Count == 0)
                    return;
                var isVmb = viewModel is ViewModelBase;
                foreach (var o in _busyMessages)
                {
                    var message = new EndBusyMessage(o.Key);
                    if (isVmb)
                        ((IHandler<object>)viewModel).Handle(this, message);
                    else
                        viewModel.Publish(this, message);
                }
            }
        }

        private void AddBusy(Guid idBusy, object message, bool notify)
        {
            InitializeBusyDict(true);
            lock (_busyMessages)
            {
                if (_busyMessages.ContainsKey(idBusy))
                    return;
                _busyMessages.Add(idBusy, message);
                BusyMessage = message;
                IsBusy = true;
            }
            OnBeginBusy(idBusy, message);
            if (notify)
                Publish(new BeginBusyMessage(idBusy, message));
        }

        private void RemoveBusy(Guid idBusy, bool notify)
        {
            if (InitializeBusyDict(false))
            {
                lock (_busyMessages)
                {
                    if (!_busyMessages.Remove(idBusy))
                        return;
                    if (_busyMessages.Count == 0)
                    {
                        IsBusy = false;
                        BusyMessage = null;
                    }
                    else
                        BusyMessage = _busyMessages.FirstOrDefault().Value;
                }
                OnEndBusy(idBusy);
            }
            if (notify)
                Publish(new EndBusyMessage(idBusy));
        }

        private void DisposeInternal()
        {
            //Disposing commands, if need.
            if (Settings.DisposeCommands)
                ReflectionExtensions.DisposeCommands(this);

            ClearBusy();
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

        private bool InitializeBusyDict(bool required)
        {
            if (_busyMessages != null)
                return true;
            if (!required)
                return false;
            Interlocked.CompareExchange(ref _busyMessages, new BusyDict(), null);
            return true;
        }

        private bool InitializeEventAggregator(bool required)
        {
            if (_localEventAggregator != null)
                return true;
            if (!required)
                return false;
            Interlocked.CompareExchange(ref _localEventAggregator, ServiceProvider.InstanceEventAggregatorFactory(this),
                null);
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
        ///     Occurs after busy operation is occurred.
        /// </summary>
        /// <param name="id">The specified id.</param>
        /// <param name="message">The specified message.</param>
        protected virtual void OnBeginBusy(Guid id, object message)
        {
        }

        /// <summary>
        ///     Occurs after busy operation is ended.
        /// </summary>
        /// <param name="id">The specified id.</param>
        protected virtual void OnEndBusy(Guid id)
        {
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