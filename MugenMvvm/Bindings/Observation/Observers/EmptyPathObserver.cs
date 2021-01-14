using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public sealed class EmptyPathObserver : IMemberPathObserver
    {
        private static readonly object Disposed = new();
        internal static readonly EmptyPathObserver Empty = new(Disposed);

        private object? _target;

        public EmptyPathObserver(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
            IsDisposable = true;
        }

        public bool IsDisposable { get; set; }

        public object? Target
        {
            get
            {
                if (_target is IWeakReference w)
                    return w.Target;
                return _target == Disposed ? null : _target;
            }
        }

        public IMemberPath Path => MemberPath.Empty;

        public bool IsAlive
        {
            get
            {
                if (_target is IWeakItem w)
                    return w.IsAlive;
                return _target != Disposed;
            }
        }

        public void Dispose()
        {
            if (IsDisposable)
                _target = Disposed;
        }

        public void AddListener(IMemberPathObserverListener listener)
        {
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
        }

        public ItemOrIReadOnlyList<IMemberPathObserverListener> GetListeners() => default;

        public MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathMembers(target, ConstantMemberInfo.Target);
        }

        public MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantMemberInfo.Target);
        }
    }
}