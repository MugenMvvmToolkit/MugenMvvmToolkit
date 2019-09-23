using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyPathObserver : IMemberPathObserver
    {
        #region Fields

        private IWeakReference? _source;

        #endregion

        #region Constructors

        public EmptyPathObserver(IWeakReference source)
        {
            Should.NotBeNull(source, nameof(source));
            _source = source;
        }

        #endregion

        #region Properties

        public bool IsAlive => _source?.Target != null;

        public IMemberPath Path => EmptyMemberPath.Instance;

        public object? Source => _source?.Target;

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
            var target = _source?.Target;
            if (target == null)
                return default;

            return new MemberPathMembers(Path, target, target, ConstantBindingMemberInfo.NullArray, ConstantBindingMemberInfo.Null);
        }

        public MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = _source?.Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(Path, target, ConstantBindingMemberInfo.Null);
        }

        public void Dispose()
        {
            _source?.Release();
            _source = null;
        }

        #endregion
    }
}