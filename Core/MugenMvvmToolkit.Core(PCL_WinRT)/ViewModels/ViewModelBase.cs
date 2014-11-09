#region Copyright
// ****************************************************************************
// <copyright file="ViewModelBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModel, IHandler<object>, IHasWeakReference
    {
        #region Fields

        private const int DisposedState = 2;
        private const int InitializedState = 1;
        private const int DefaultState = 0;

        private readonly Dictionary<Guid, object> _busyCollection;
        private readonly IEventAggregator _viewModelEventAggregator;
        private readonly IViewModelSettings _settings;

        private CancellationTokenSource _disposeCancellationToken;
        private IIocContainer _iocContainer;
        private IViewModelProvider _viewModelProvider;
        private IThreadManager _threadManager;
        private WeakReference _weakReference;
        private object _busyMessage;

        private int _state;
        private bool _isBusy;
        private bool _isRestored;
        private bool _isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelBase" /> class.
        /// </summary>
        protected ViewModelBase()
        {
            _busyCollection = new Dictionary<Guid, object>();
            _disposeCancellationToken = new CancellationTokenSource();
            _settings = ApplicationSettings.ViewModelSettings.Clone();
            _viewModelEventAggregator = ServiceProvider.InstanceEventAggregatorFactory(this);

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
        /// Gets the current <see cref="IEventAggregator"/>.
        /// </summary>
        protected internal IEventAggregator ViewModelEventAggregator
        {
            get { return _viewModelEventAggregator; }
        }

        /// <summary>
        ///     Gets or sets the default view model provider to create view models.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider ?? ServiceProvider.ViewModelProvider; }
            set
            {
                Should.PropertyBeNotNull(value);
                _viewModelProvider = value;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is restored.
        /// </summary>
        protected bool IsRestored
        {
            get { return _isRestored; }
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
                if (IsDisposed)
                    return new CancellationToken(true);
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
            get { return _state == InitializedState; }
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
                OnPropertyChanged("IsDisposed");
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
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
                if (Settings.ThrowOnMultiInitialization)
                    throw ExceptionManager.ObjectInitialized("ViewModel", this);
                return;
            }
            context.TryGetData(InitializationConstants.IsRestored, out _isRestored);
            IocContainer = context.GetData(InitializationConstants.IocContainer, true);
            _threadManager = _iocContainer.Get<IThreadManager>();
            if (_viewModelProvider == null)
                ViewModelProvider = _iocContainer.Get<IViewModelProvider>();

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
            Guid[] guids;
            lock (_busyCollection)
                guids = _busyCollection.Keys.ToArrayEx();
            for (int index = 0; index < guids.Length; index++)
                EndBusy(guids[index]);
        }

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        public bool Subscribe(object instance)
        {
            if (!SubscribeInternal(instance))
                return false;
            var vm = instance as IViewModel;
            if (vm != null)
                NotifyBeginBusy(vm);
            return true;
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        public bool Unsubscribe(object instance)
        {
            if (!UnsubscribeInternal(instance))
                return false;
            var vm = instance as IViewModel;
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
                _isDisposed = true;
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
            HandleInternal(sender, message);
            OnHandle(sender, message);
        }

        /// <summary>
        ///     Gets the <see cref="System.WeakReference" /> of current object.
        /// </summary>
        WeakReference IHasWeakReference.WeakReference
        {
            get
            {
                if (_weakReference == null)
                    _weakReference = ServiceProvider.WeakReferenceFactory(this, true);
                return _weakReference;
            }
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
                if (messageMode.HasFlagEx(HandleMode.NotifyObservers))
                    Publish(sender, message);
                return;
            }
            var endBusyMessage = message as EndBusyMessage;
            if (endBusyMessage != null)
            {
                RemoveBusy(endBusyMessage.Id, false);
                Publish(sender, message);
                return;
            }
            if (Settings.BroadcastAllMessages || message is IBroadcastMessage)
                Publish(sender, message);
        }

        private void NotifyBeginBusy(IViewModel viewModel)
        {
            lock (_busyCollection)
            {
                if (_busyCollection.Count == 0)
                    return;
                var isVmb = viewModel is ViewModelBase;
                foreach (var o in _busyCollection)
                {
                    var message = new BeginBusyMessage(o.Key, o.Value);
                    if (isVmb)
                        ((IHandler<object>)viewModel).Handle(this, message);
                    else
                        EventAggregator.Publish(viewModel, this, message);
                }
            }
        }

        private void NotifyEndBusy(IViewModel viewModel)
        {
            lock (_busyCollection)
            {
                if (_busyCollection.Count == 0)
                    return;
                var isVmb = viewModel is ViewModelBase;
                foreach (Guid o in _busyCollection.Keys)
                {
                    var message = new EndBusyMessage(o);
                    if (isVmb)
                        ((IHandler<object>)viewModel).Handle(this, message);
                    else
                        EventAggregator.Publish(viewModel, this, message);
                }
            }
        }

        private void AddBusy(Guid idBusy, object message, bool notify)
        {
            lock (_busyCollection)
            {
                if (_busyCollection.ContainsKey(idBusy))
                    return;
                _busyCollection.Add(idBusy, message);
                BusyMessage = message;
                IsBusy = true;
            }
            OnBeginBusy(idBusy, message);
            if (notify)
                Publish(new BeginBusyMessage(idBusy, message));
        }

        private void RemoveBusy(Guid idBusy, bool notify)
        {
            lock (_busyCollection)
            {
                if (!_busyCollection.Remove(idBusy))
                    return;
                if (_busyCollection.Count == 0)
                {
                    IsBusy = false;
                    BusyMessage = null;
                }
                else
                    BusyMessage = _busyCollection.Values.FirstOrDefault();
            }
            OnEndBusy(idBusy);
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
            var toRemove = _viewModelEventAggregator.GetObservers();
            for (int index = 0; index < toRemove.Count; index++)
                Unsubscribe(toRemove[index]);

            IViewManager viewManager;
            if (IocContainer.TryGet(out viewManager))
                viewManager.CleanupViewAsync(this);

            _viewModelProvider = null;
            //Disposing ioc adapter.
            if (Settings.DisposeIocContainer && _iocContainer != null)
                _iocContainer.Dispose();

            _disposeCancellationToken.Cancel();
            Tracer.TraceViewModel(AuditAction.Disposed, this);
            Settings.Metadata.Clear();
        }

        #endregion

        #region Overrides of NotifyPropertyChangedBase

        /// <summary>
        ///     Gets or sets the <see cref="IThreadManager" />.
        /// </summary>
        protected override IThreadManager ThreadManager
        {
            get
            {
                if (_threadManager == null)
                    return base.ThreadManager;
                return _threadManager;
            }
        }

        internal override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            Publish(StateChangedMessage.Empty);
        }

        #endregion

        #region Virtual methods

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        protected virtual bool SubscribeInternal(object instance)
        {
            return instance != this && _viewModelEventAggregator.Subscribe(instance);
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        protected virtual bool UnsubscribeInternal(object instance)
        {
            return instance != this && _viewModelEventAggregator.Unsubscribe(instance);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="sender">The specified sender.</param>
        /// <param name="message">The message instance.</param>
        protected virtual void Publish(object sender, object message)
        {
            Should.NotBeNull(sender, "sender");
            Should.NotBeNull(message, "message");
            _viewModelEventAggregator.Publish(sender, message);
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