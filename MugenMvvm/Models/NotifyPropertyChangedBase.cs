using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Models
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged, ISuspendable, IValueHolder<IWeakReference>, IValueHolder<IDictionary<string, object?>>,
        IValueHolder<MemberListenerCollection>
    {
        [NonSerialized]
        [IgnoreDataMember]
        private bool _isNotificationsDirty;

        [NonSerialized]
        [IgnoreDataMember]
        private MemberListenerCollection? _memberListeners;

        [NonSerialized]
        [IgnoreDataMember]
        private volatile int _suspendCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        [IgnoreDataMember]
        public bool IsSuspended => _suspendCount != 0;

        [IgnoreDataMember]
        internal bool HasSubscribers => PropertyChanged != null;

        [IgnoreDataMember]
        [field: NonSerialized]
        IDictionary<string, object?>? IValueHolder<IDictionary<string, object?>>.Value { get; set; }

        [IgnoreDataMember]
        [field: NonSerialized]
        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        MemberListenerCollection? IValueHolder<MemberListenerCollection>.Value
        {
            get => _memberListeners;
            set => _memberListeners = value;
        }

        public void InvalidateProperties() => OnPropertyChanged(Default.EmptyPropertyChangedArgs);

        public ActionToken Suspend(IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate((m, _) => ((NotifyPropertyChangedBase) m!).EndSuspend(), this);
        }

        protected virtual void OnEndSuspend(bool isDirty)
        {
            if (isDirty)
                InvalidateProperties();
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (IsSuspended)
                _isNotificationsDirty = true;
            else
            {
                _memberListeners?.RaisePropertyChanged(this, args);
                PropertyChanged?.Invoke(this, args);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void ClearPropertyChangedSubscribers()
        {
            _memberListeners = null;
            PropertyChanged = null;
        }

        internal void OnPropertyChangedInternal(PropertyChangedEventArgs args) => OnPropertyChanged(args);

        private void EndSuspend()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0)
            {
                OnEndSuspend(_isNotificationsDirty);
                _isNotificationsDirty = false;
            }
        }
    }
}