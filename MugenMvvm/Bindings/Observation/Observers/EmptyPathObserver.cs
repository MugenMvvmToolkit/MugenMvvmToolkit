using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public sealed class EmptyPathObserver : IMemberPathObserver
    {
        #region Fields

        private object? _target;

        private static readonly object Disposed = new();
        internal static readonly EmptyPathObserver Empty = new(Disposed);

        #endregion

        #region Constructors

        public EmptyPathObserver(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
            IsDisposable = true;
        }

        #endregion

        #region Properties

        public bool IsAlive
        {
            get
            {
                if (_target is IWeakItem w)
                    return w.IsAlive;
                return _target != Disposed;
            }
        }

        public object? Target
        {
            get
            {
                if (_target is IWeakReference w)
                    return w.Target;
                return _target == Disposed ? null : _target;
            }
        }

        public IMemberPath Path => EmptyMemberPath.Instance;

        public bool IsDisposable { get; set; }

        #endregion

        #region Implementation of interfaces

        public void AddListener(IMemberPathObserverListener listener)
        {
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
        }

        public ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> GetListeners() => default;

        public MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathMembers(target, ConstantMemberInfo.TargetArray);
        }

        public MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantMemberInfo.Target);
        }

        public void Dispose()
        {
            if (IsDisposable)
                _target = Disposed;
        }

        #endregion
    }
}