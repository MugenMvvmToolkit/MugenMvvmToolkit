using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EmptyPathObserver : IBindingPathObserver
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

        public IBindingPath Path => EmptyBindingPath.Instance;

        public object? Source => _source?.Target;

        #endregion

        #region Implementation of interfaces

        public void AddListener(IBindingPathObserverListener listener)
        {
        }

        public void RemoveListener(IBindingPathObserverListener listener)
        {
        }

        public BindingPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var target = _source?.Target;
            if (target == null)
                return default;

            return new BindingPathMembers(Path, target, target, ConstantBindingMemberInfo.NullArray, ConstantBindingMemberInfo.Null);
        }

        public BindingPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = _source?.Target;
            if (target == null)
                return default;

            return new BindingPathLastMember(Path, target, ConstantBindingMemberInfo.Null);
        }

        public void Dispose()
        {
            _source?.Release();
            _source = null;
        }

        #endregion
    }
}