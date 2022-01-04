using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public abstract class ObserverBase : IMemberPathObserver//todo chain without boxing
    {
        protected const byte UpdatingFlag = 1 << 1;
        protected const byte OptionalFlag = 1 << 2;
        protected const byte HasStablePathFlag = 1 << 3;//todo use attribute, smart detect
        protected const byte InitializedFlag = 1 << 4;
        protected const byte NoDisposeFlag = 1 << 5;
        protected const byte WeakFlag = 1 << 6;

        private static readonly IMemberPathObserverListener[] DisposedItems = new IMemberPathObserverListener[0];

        private readonly ushort _flags;

        private object? _listeners;
        private byte _state;
        private object? _target;

        protected ObserverBase(object target, EnumFlags<MemberFlags> memberFlags)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
            _flags = memberFlags.Value();
        }

        public abstract IMemberPath Path { get; }

        public EnumFlags<MemberFlags> MemberFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(_flags);
        }

        public bool HasStablePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CheckFlag(HasStablePathFlag);
        }

        public bool Optional
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CheckFlag(OptionalFlag);
        }

        public bool IsWeak
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CheckFlag(WeakFlag);
        }

        public bool IsDisposable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !CheckFlag(NoDisposeFlag);
            set
            {
                if (value)
                    ClearFlag(NoDisposeFlag);
                else
                    SetFlag(NoDisposeFlag);
            }
        }

        public object? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_target is IWeakReference w)
                    return w.Target;
                return _target;
            }
        }

        public bool IsAlive
        {
            get
            {
                if (_target is IWeakItem w)
                    return w.IsAlive;
                return !IsDisposed;
            }
        }

        protected bool HasListeners
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _listeners != null;
        }

        protected bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _listeners == DisposedItems;
        }

        public abstract MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        public abstract MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);

        public void Dispose()
        {
            if (_listeners == DisposedItems || !IsDisposable)
                return;
            lock (this)
            {
                if (_listeners == DisposedItems || !IsDisposable)
                    return;
                _listeners = DisposedItems;
                _target = null;
            }

            OnDisposed();
        }

        public void AddListener(IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (IsDisposed)
                return;

            object? oldListeners;
            (bool, Exception?) state;
            lock (this)
            {
                if (IsDisposed)
                    return;
                oldListeners = _listeners;
                MugenExtensions.AddRaw(ref _listeners, listener);
                state = oldListeners == null ? OnListenersAdded() : default;
            }

            if (oldListeners == null && state.Item1)
            {
                if (state.Item2 != null)
                    OnError(state.Item2);
                RaiseOnListenersAdded();//todo expression binding is called twice on attach
            }
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (IsDisposed)
                return;
            lock (this)
            {
                if (!IsDisposed && MugenExtensions.RemoveRaw(ref _listeners, listener) && _listeners == null)
                    OnListenersRemoved();
            }
        }

        public ItemOrIReadOnlyList<IMemberPathObserverListener> GetListeners()
        {
            if (IsDisposed)
                return default;
            return ItemOrIReadOnlyList.FromRawValue<IMemberPathObserverListener>(_listeners);
        }

        protected virtual (bool, Exception?) OnListenersAdded() => default;

        protected virtual void RaiseOnListenersAdded()
        {
        }

        protected virtual void OnListenersRemoved()
        {
        }

        protected virtual void OnDisposed()
        {
        }

        protected virtual void OnLastMemberChanged()
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    foreach (var t in l)
                        t.OnLastMemberChanged(this);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnLastMemberChanged(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected void OnPathMembersChanged()
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    foreach (var t in l)
                        t.OnPathMembersChanged(this);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnPathMembersChanged(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected void OnError(Exception exception)
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    foreach (var t in l)
                        t.OnError(this, exception);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnError(this, exception);
            }
            catch
            {
                // ignored
            }
        }

        protected IReadOnlyMetadataContext? TryGetMetadata()
        {
            if (_listeners == null)
                return null;
            if (_listeners is IMemberPathObserverListener[] l)
            {
                foreach (var t in l)
                {
                    var metadata = TryGetMetadata(t);
                    if (metadata != null)
                        return metadata;
                }

                return null;
            }

            return TryGetMetadata(_listeners);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlag(byte flag) => (_state & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(byte flag) => _state = (byte)(_state | flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearFlag(byte flag) => _state = (byte)(_state & ~flag);

        private static IReadOnlyMetadataContext? TryGetMetadata(object? value)
        {
            if (value is IMetadataOwner<IReadOnlyMetadataContext> metadataOwner && metadataOwner.HasMetadata)
                return metadataOwner.Metadata;
            return null;
        }

        protected internal interface IMethodPathObserver : IMemberPathObserver
        {
            EnumFlags<MemberFlags> MemberFlags { get; }

            string Method { get; }

            IEventListener GetMethodListener();
        }
    }
}