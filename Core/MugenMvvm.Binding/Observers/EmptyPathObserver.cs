using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyPathObserver : IMemberPathObserver
    {
        #region Fields

        private IWeakReference? _target;

        #endregion

        #region Constructors

        public EmptyPathObserver(IWeakReference target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
        }

        #endregion

        #region Properties

        public bool IsAlive => _target?.Target != null;

        public IMemberPath Path => EmptyMemberPath.Instance;

        public object? Target => _target?.Target;

        #endregion

        #region Implementation of interfaces

        public void AddListener(IMemberPathObserverListener listener)
        {
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
        }

        public MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var target = _target?.Target;
            if (target == null)
                return default;

            return new MemberPathMembers(target, ConstantBindingMemberInfo.NullArray);
        }

        public MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = _target?.Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantBindingMemberInfo.Null);
        }

        public void Dispose()
        {
            _target = null;
        }

        #endregion
    }
}