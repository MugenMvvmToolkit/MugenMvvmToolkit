using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Models
{
    public class NotifyPropertyChangedBase : INotifyPropertyChangedEx, IHasWeakReference
    {
        #region Fields

        private DispatcherHandler? _handler;
        private WeakReference? _ref;
        private int _suspendCount;
        internal LightDictionaryBase<string, object?>? AttachedValues;

        #endregion

        #region Properties

        public bool IsNotificationsSuspended => _suspendCount != 0;

        WeakReference IHasWeakReference.WeakReference
        {
            get
            {
                if (_ref == null)
                    MugenExtensions.LazyInitialize(ref _ref, MugenExtensions.GetWeakReference(this, true));
                return _ref!;
            }
        }

        protected bool IsNotificationsDirty { get; set; }

        protected virtual ThreadExecutionMode PropertyChangedExecutionMode => ThreadExecutionMode.Main;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public IDisposable SuspendNotifications()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnBeginSuspendNotifications();
            return WeakActionToken.Create(this, @base => @base.EndSuspendNotifications());
        }

        public void InvalidateProperties()
        {
            OnPropertyChanged(Default.EmptyPropertyChangedArgs);
        }

        #endregion

        #region Methods

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                OnPropertyChanged(Default.GetOrCreatePropertyChangedArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                SetProperty(ref field, newValue, Default.GetOrCreatePropertyChangedArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T newValue, PropertyChangedEventArgs args)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(args);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            if (PropertyChanged != null)
            {
                if (IsNotificationsSuspended)
                    IsNotificationsDirty = true;
                if (PropertyChangedExecutionMode == ThreadExecutionMode.Current)
                    RaisePropertyChangedEvent(args);
                else if (PropertyChangedExecutionMode == ThreadExecutionMode.Main && Service<IThreadDispatcher>.Instance.IsOnMainThread)
                    RaisePropertyChangedEvent(args);
                else
                    Service<IThreadDispatcher>.Instance.Execute(GetDispatcherHandler(), PropertyChangedExecutionMode, args);
            }
        }

        protected virtual void OnBeginSuspendNotifications()
        {
        }

        protected virtual void OnEndSuspendNotifications()
        {
        }

        protected void ClearPropertyChangedSubscribers()
        {
            PropertyChanged = null!;
        }

        protected void CleanupWeakReference()
        {
            if (_ref != null)
                _ref.Target = null;
        }

        protected virtual void RaisePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            OnEndSuspendNotifications();
            if (IsNotificationsDirty)
            {
                IsNotificationsDirty = false;
                InvalidateProperties();
            }

            OnPropertyChanged(Default.IsNotificationsSuspendedChangedArgs);
        }

        private protected DispatcherHandler GetDispatcherHandler()
        {
            if (_handler == null)
                MugenExtensions.LazyInitialize(ref _handler, CreateDispatcherHandler());
            return _handler!;
        }

        private protected virtual DispatcherHandler CreateDispatcherHandler()
        {
            return new DispatcherHandler(this);
        }

        #endregion

        #region Nested types

        private protected class DispatcherHandler : IThreadDispatcherHandler
        {
            #region Fields

            protected readonly NotifyPropertyChangedBase Target;

            #endregion

            #region Constructors

            public DispatcherHandler(NotifyPropertyChangedBase target)
            {
                Target = target;
            }

            #endregion

            #region Implementation of interfaces

            public virtual void Execute(object? state)
            {
                Target.RaisePropertyChangedEvent((PropertyChangedEventArgs)state!);
            }

            #endregion
        }

        #endregion
    }
}