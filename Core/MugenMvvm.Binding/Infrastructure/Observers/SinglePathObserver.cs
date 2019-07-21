using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class SinglePathObserver : ObserverBase, IBindingEventListener, IWeakReferenceHolder
    {
        #region Fields

        private readonly bool _hasStablePath;
        private readonly bool _ignoreAttachedMembers;
        private readonly bool _observable;
        private readonly bool _optional;
        private Exception? _exception;
        private IBindingMemberInfo? _lastMember;
        private IDisposable? _lastMemberUnsubscriber;
        private IWeakReference? _penultimateValue;

        #endregion

        #region Constructors

        public SinglePathObserver(IWeakReference source, IBindingMemberInfo? member, IBindingPath path,
            bool ignoreAttachedMembers, bool hasStablePath, bool observable, bool optional)
            : base(source, member)
        {
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _hasStablePath = hasStablePath;
            _observable = observable;
            _optional = optional;
            Path = path;
        }

        #endregion

        #region Properties

        public override IBindingPath Path { get; }

        public bool IsWeak => false;

        public IWeakReference? WeakReference { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IBindingEventListener.TryHandle(object sender, object? message)
        {
            OnLastMemberChanged();
            return true;
        }

        #endregion

        #region Methods

        public override BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathMembers(Path, _exception);
            var target = _penultimateValue?.Target;
            if (target == null || _lastMember == null)
                return default;
            return new BindingPathMembers(Path, target, target, null, _lastMember);
        }

        public override BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathLastMember(Path, _exception);
            var target = _penultimateValue?.Target;
            if (target == null || _lastMember == null)
                return default;
            return new BindingPathLastMember(Path, target, _lastMember);
        }

        protected override IBindingEventListener GetSourceListener()
        {
            return new SourceListener(Service<IWeakReferenceProvider>.Instance.GetWeakReference(this, Default.Metadata));
        }

        protected override void OnListenerAdded(IBindingPathObserverListener listener)
        {
            UpdateIfNeed();
            if (_observable && _lastMemberUnsubscriber == null && _penultimateValue != null)
            {
                var target = _penultimateValue.Target;
                if (target != null)
                    Subscribe(target, _lastMember);
            }
        }

        protected override void OnDisposed()
        {
            Unsubscribe();
            _penultimateValue?.Release();
            _penultimateValue = null;
            _lastMember = null;
            _exception = null;
        }

        private void UpdateIfNeed()
        {
            if (!HasSourceListener)
            {
                AddSourceListener();
                Update();
            }
        }

        private bool Update()
        {
            try
            {
                if (!TryGetSourceValue(out var source))
                    return false;

                if (source == null)
                {
                    SetLastMember(null, null, null);
                    return true;
                }

                if (_penultimateValue != null)
                {
                    if (ReferenceEquals(_penultimateValue.Target, source))
                        return true;

                    if (_hasStablePath)
                    {
                        Unsubscribe();
                        if (_observable && HasListeners)
                            Subscribe(source, _lastMember);
                        SetLastMember(GetSourceWeakReference(source), _lastMember, null);
                        return true;
                    }
                }

                Unsubscribe();
                var lastMember = GetBindingMember(source.GetType(), Path.Path, _ignoreAttachedMembers);
                if (lastMember == null)
                {
                    if (_optional)
                        SetLastMember(null, null, null);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), Path.Path);
                    return true;
                }

                if (_observable && HasListeners)
                    Subscribe(source, lastMember);

                SetLastMember(GetSourceWeakReference(source), lastMember, null);
            }
            catch (Exception e)
            {
                SetLastMember(null, null, e);
                OnError(e);
            }

            return true;
        }

        private void SetLastMember(IWeakReference? penultimateValue, IBindingMemberInfo? lastMember, Exception? exception)
        {
            _penultimateValue = penultimateValue;
            _lastMember = lastMember;
            _exception = exception;
            OnPathMembersChanged();
        }

        private void Subscribe(object source, IBindingMemberInfo? lastMember)
        {
            _lastMemberUnsubscriber?.Dispose();
            _lastMemberUnsubscriber = lastMember?.TryObserve(source, this, null) ?? Default.Disposable;
        }

        private void Unsubscribe()
        {
            _lastMemberUnsubscriber?.Dispose();
            _lastMemberUnsubscriber = null;
        }

        #endregion

        #region Nested types

        private sealed class SourceListener : IBindingEventListener
        {
            #region Fields

            private readonly IWeakReference _observer;

            #endregion

            #region Constructors

            public SourceListener(IWeakReference observer)
            {
                _observer = observer;
            }

            #endregion

            #region Properties

            public bool IsAlive => _observer.Target != null;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            public bool TryHandle(object sender, object? message)
            {
                var observer = (SinglePathObserver?)_observer.Target;
                if (observer == null)
                    return false;
                return observer.Update();
            }

            #endregion
        }

        #endregion
    }
}