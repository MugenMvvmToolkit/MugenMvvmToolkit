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
        [NonSerialized]
        [IgnoreDataMember]
        private bool _isNotificationsDirty;

        [NonSerialized]
        [IgnoreDataMember]
        private MemberListenerCollection? _memberListeners;

        [NonSerialized]
        [IgnoreDataMember]
        private int _suspendCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSuspended => _suspendCount != 0;

        [IgnoreDataMember]
        [field: NonSerialized]
        Delegate? IValueHolder<Delegate>.Value { get; set; }

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

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((m, _) => ((NotifyPropertyChangedBase) m!).EndSuspend(), this);
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

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (IsSuspended)
                _isNotificationsDirty = true;
            else
                MugenExtensions.DefaultIfNull<IThreadDispatcher>(null, this).Execute(ThreadExecutionMode.Main, this, args);
        }

        protected void ClearPropertyChangedSubscribers()
        {
            _memberListeners = null;
            PropertyChanged = null;
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

        void IThreadDispatcherHandler.Execute(object? state) => OnPropertyChangedInternal((PropertyChangedEventArgs) state!);
    }
}