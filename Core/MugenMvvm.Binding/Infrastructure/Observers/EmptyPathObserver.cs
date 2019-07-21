using MugenMvvm.Binding.Infrastructure.Members;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class EmptyPathObserver : ObserverBase, IBindingEventListener, IWeakReferenceHolder
    {
        #region Constructors

        public EmptyPathObserver(IWeakReference source, IBindingMemberInfo? member) 
            : base(source, member)
        {
        }

        #endregion

        #region Properties

        public bool IsWeak => false;

        public override IBindingPath Path => EmptyBindingPath.Instance;

        public IWeakReference? WeakReference { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IBindingEventListener.TryHandle(object sender, object? message)
        {
            var source = Source;
            if (source == null)
                return false;
            OnLastMemberChanged();
            return true;
        }

        #endregion

        #region Methods

        public override BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata)
        {
            if (TryGetSourceValue(out var source))
                return new BindingPathMembers(Path, source, source, ConstantBindingMemberInfo.NullInstanceArray, ConstantBindingMemberInfo.NullInstance);
            return default;
        }

        public override BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata)
        {
            if (TryGetSourceValue(out var source))
                return new BindingPathLastMember(Path, source, ConstantBindingMemberInfo.NullInstance);
            return default;
        }

        protected override void OnListenerAdded(IBindingPathObserverListener listener)
        {
            if (!HasSourceListener)
                AddSourceListener();
        }

        protected override IBindingEventListener GetSourceListener()
        {
            return this;
        }

        #endregion
    }
}