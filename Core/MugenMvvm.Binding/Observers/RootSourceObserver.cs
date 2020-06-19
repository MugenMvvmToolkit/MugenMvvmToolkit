using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class RootSourceObserver : EventListenerCollection, IWeakEventListener
    {
        #region Fields

        private readonly IWeakReference _targetRef;
        private IWeakReference? _parentRef;
        private ActionToken _parentToken;

        #endregion

        #region Constructors

        private RootSourceObserver(object target, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            _targetRef = target.ToWeakReference();
            UpdateParent(target, metadata);
        }

        #endregion

        #region Properties

        public bool IsAlive => _targetRef.IsAlive;

        public bool IsWeak => true;

        #endregion

        #region Implementation of interfaces

        public bool TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            var target = _targetRef.Target;
            if (target == null)
                return false;
            if (TypeChecker.IsValueType<T>() || !(message is RootSourceObserver))
            {
                if (UpdateParent(target, metadata))
                    Raise(target, this, metadata);
            }
            else
                Raise(target, message, metadata);

            return true;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            var parent = _parentRef?.Target;
            if (parent != null)
                GetOrAdd(parent, null).Remove(this);
            _parentRef = Default.WeakReference;
            _parentToken.Dispose();
            Clear();
        }

        public static RootSourceObserver GetOrAdd(object target, IReadOnlyMetadataContext? metadata)
        {
            return MugenService.AttachedValueProvider.GetOrAdd(target, BindingInternalConstant.RootObserver, metadata, (o, m) => new RootSourceObserver(o, m));
        }

        public static void Clear(object target)
        {
            if (MugenService.AttachedValueProvider.Clear(target, BindingInternalConstant.RootObserver, out var value))
                (value as RootSourceObserver)?.Dispose();
        }

        public object? GetRoot(IReadOnlyMetadataContext? metadata)
        {
            return GetRoot(_targetRef.Target, metadata);
        }

        public object? GetRoot(object? target, IReadOnlyMetadataContext? metadata)
        {
            var parent = _parentRef?.Target;
            if (parent == null)
                return target;
            return GetOrAdd(parent, metadata).GetRoot(parent, metadata);
        }

        private bool UpdateParent(object target, IReadOnlyMetadataContext? metadata)
        {
            var member = MugenBindingService
                .MemberManager
                .TryGetMember(target.GetType(), MemberType.Accessor, MemberFlags.InstanceAll, BindableMembers.Object.Parent.Name, metadata) as IAccessorMemberInfo;
            var oldParent = _parentRef?.Target;
            var parent = member?.GetValue(target, metadata);
            if (oldParent == parent)
            {
                if (member != null && _parentRef == null && _parentToken.IsEmpty)
                    _parentToken = member.TryObserve(target, this, metadata);
                return false;
            }

            if (member != null && _parentToken.IsEmpty)
                _parentToken = member.TryObserve(target, this, metadata);
            if (oldParent != null)
                GetOrAdd(oldParent, metadata).Remove(this);
            if (parent != null)
                GetOrAdd(parent, metadata).Add(this);
            _parentRef = parent?.ToWeakReference();
            return true;
        }

        #endregion
    }
}