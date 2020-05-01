using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers.PathObservers
{
    public sealed class EmptyPathObserver : IMemberPathObserver
    {
        #region Fields

        private object? _target;

        internal static readonly EmptyPathObserver Empty = new EmptyPathObserver(Default.WeakReference);

        #endregion

        #region Constructors

        public EmptyPathObserver(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
        }

        #endregion

        #region Properties

        public bool IsAlive
        {
            get
            {
                if (_target is IWeakReference w)
                    return w.Target != null;
                return true;
            }
        }

        public object? Target
        {
            get
            {
                if (_target is IWeakReference w)
                    return w.Target;
                return _target;
            }
        }

        public IMemberPath Path => EmptyMemberPath.Instance;

        #endregion

        #region Implementation of interfaces

        public void AddListener(IMemberPathObserverListener listener)
        {
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
        }

        public ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> GetListeners()
        {
            return default;
        }

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
            _target = null;
        }

        #endregion
    }
}