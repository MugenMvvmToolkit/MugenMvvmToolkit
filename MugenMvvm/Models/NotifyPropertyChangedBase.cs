using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Models
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged, IThreadDispatcherHandler, ISuspendable,
        IValueHolder<IWeakReference>, IValueHolder<Delegate>, IValueHolder<IDictionary<string, object?>>, IValueHolder<MemberListenerCollection>
    {
        #region Fields

        [NonSerialized]
        [IgnoreDataMember]
        private bool _isNotificationsDirty;

        [NonSerialized]
        [IgnoreDataMember]
        private int _suspendCount;

        [NonSerialized]
        [IgnoreDataMember]
        private MemberListenerCollection? _memberListeners;

        #endregion

        #region Properties

        [IgnoreDataMember]
        [field: NonSerialized]
        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        [IgnoreDataMember]
        [field: NonSerialized]
        Delegate? IValueHolder<Delegate>.Value { get; set; }

        [IgnoreDataMember]
        [field: NonSerialized]
        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        public bool IsSuspended => _suspendCount != 0;

        MemberListenerCollection? IValueHolder<MemberListenerCollection>.Value
        {
            get => _memberListeners;
            set => _memberListeners = value;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((m, _) => ((NotifyPropertyChangedBase) m!).EndSuspend(), this);
        }

        void IThreadDispatcherHandler.Execute(object? state) => OnPropertyChangedInternal((PropertyChangedEventArgs) state!);

        #endregion

        #region Methods

        public void InvalidateProperties() => OnPropertyChanged(Default.EmptyPropertyChangedArgs);

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (IsSuspended)
                _isNotificationsDirty = true;
            else
                MugenService.ThreadDispatcher.Execute(ThreadExecutionMode.Main, this, args);
        }

        protected void ClearPropertyChangedSubscribers()
        {
            _memberListeners = null;
            PropertyChanged = null;
        }

        protected virtual void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            _memberListeners?.RaisePropertyChanged(this, args);
            PropertyChanged?.Invoke(this, args);
        }

        protected virtual void OnEndSuspend(bool isDirty)
        {
            if (isDirty)
                InvalidateProperties();
        }

        private void EndSuspend()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0)
            {
                OnEndSuspend(_isNotificationsDirty);
                if (_isNotificationsDirty)
                    _isNotificationsDirty = false;
            }
        }

        #endregion
    }
}