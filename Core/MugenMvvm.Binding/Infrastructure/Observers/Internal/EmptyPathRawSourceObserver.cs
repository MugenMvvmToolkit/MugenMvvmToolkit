using MugenMvvm.Binding.Infrastructure.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding.Infrastructure.Observers
{
    internal sealed class EmptyPathRawSourceObserver : IBindingPathObserver
    {
        #region Fields

        private IWeakReference? _source;

        #endregion

        #region Constructors

        public EmptyPathRawSourceObserver(IWeakReference source)
        {
            _source = source;
        }

        #endregion

        #region Properties

        public bool IsAlive => _source?.Target != null;

        public IBindingPath Path => EmptyBindingPath.Instance;

        public object Source => _source?.Target;

        #endregion

        #region Implementation of interfaces

        public void AddListener(IBindingPathObserverListener listener)
        {
        }

        public void RemoveListener(IBindingPathObserverListener listener)
        {
        }

        public BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata)
        {
            var target = _source?.Target;
            if (target == null)
                return default;

            return new BindingPathMembers(Path, target, target, ConstantBindingMemberInfo.NullInstanceArray, ConstantBindingMemberInfo.NullInstance);
        }

        public BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata)
        {
            var target = _source?.Target;
            if (target == null)
                return default;

            return new BindingPathLastMember(Path, target, ConstantBindingMemberInfo.NullInstance);
        }

        public void Dispose()
        {
            _source?.Release();
            _source = null;
        }

        #endregion
    }
}