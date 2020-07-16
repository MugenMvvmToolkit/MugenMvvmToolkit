using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation
{
    public sealed class RootSourceObserver : EventListenerCollection, IWeakEventListener
    {
        #region Fields

        private readonly IWeakReference _targetRef;
        private IWeakReference? _parentRef;
        private ActionToken _parentToken;

        #endregion

        #region Constructors

        private RootSourceObserver(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _targetRef = target.ToWeakReference();
        }

        #endregion

        #region Properties

        public bool IsAlive => _targetRef.IsAlive;

        public bool IsWeak => true;

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
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
            ClearParent();
            Clear();
        }

        public static RootSourceObserver GetOrAdd(object target)
        {
            return (RootSourceObserver)MugenService.AttachedValueManager.GetOrAdd(target, BindingInternalConstant.RootObserver, (o, _) => new RootSourceObserver(o))!;
        }

        public static void Clear(object target)
        {
            if (MugenService.AttachedValueManager.Clear(target, BindingInternalConstant.RootObserver, out var value))
                (value as RootSourceObserver)?.Dispose();
        }

        public object? Get(IReadOnlyMetadataContext? metadata)
        {
            return Get(_targetRef.Target, metadata);
        }

        [return: NotNullIfNotNull("target")]
        public object? Get(object? target, IReadOnlyMetadataContext? metadata)
        {
            return MugenBindingExtensions.GetRoot(target, metadata);
        }

        protected override void OnListenersAdded()
        {
            var target = _targetRef.Target;
            if (target != null)
                UpdateParent(target, null);
        }

        protected override void OnListenersRemoved()
        {
            ClearParent();
        }

        private bool UpdateParent(object target, IReadOnlyMetadataContext? metadata)
        {
            var member = BindableMembers.For<object>().Parent().TryGetMember(target.GetType(), MemberFlags.InstancePublicAll, metadata);
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
                GetOrAdd(oldParent).Remove(this);
            if (parent != null)
                GetOrAdd(parent).Add(this);
            _parentRef = parent?.ToWeakReference();
            return true;
        }

        private void ClearParent()
        {
            _parentToken.Dispose();
            var parent = _parentRef?.Target;
            if (parent != null)
            {
                GetOrAdd(parent).Remove(this);
                _parentRef = Default.WeakReference;
            }
        }

        #endregion
    }
}