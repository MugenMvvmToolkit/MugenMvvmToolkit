using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MethodSinglePathObserver : SinglePathObserver
    {
        #region Fields

        private readonly string _observableMethodName;

        private IWeakReference? _lastValueRef;
        private IDisposable? _unsubscriber;

        #endregion

        #region Constructors

        public MethodSinglePathObserver(string observableMethodName, object source, IMemberPath path, MemberFlags memberFlags, bool optional)
            : base(source, path, memberFlags, true, optional)
        {
            Should.NotBeNull(observableMethodName, nameof(observableMethodName));
            _observableMethodName = observableMethodName;
        }

        #endregion

        #region Methods

        protected override void SubscribeLastMember(object source, IBindingMemberInfo? lastMember)
        {
            base.SubscribeLastMember(source, lastMember);
            AddMethodObserver(source, lastMember);
        }

        protected override void OnLastMemberChanged()
        {
            base.OnLastMemberChanged();
            var lastMember = GetLastMember();
            AddMethodObserver(lastMember.Source, lastMember.LastMember);
        }

        protected override void UnsubscribeLastMember()
        {
            _unsubscriber?.Dispose();
            _unsubscriber = null;
        }

        private void AddMethodObserver(object? source, IBindingMemberInfo? lastMember)
        {
            _unsubscriber?.Dispose();
            if (source == null || !(lastMember is IBindingPropertyInfo propertyInfo))
            {
                _unsubscriber = Default.Disposable;
                return;
            }

            var value = propertyInfo.GetValue(source);
            if (ReferenceEquals(value, _lastValueRef?.Target))
                return;

            var type = value?.GetType()!;
            if (value.IsNullOrUnsetValue() || type.IsValueTypeUnified())
            {
                _unsubscriber = Default.Disposable;
                return;
            }

            _lastValueRef = value.ToWeakReference();
            var memberFlags = MemberFlags & ~MemberFlags.Static;
            var member = MugenBindingService.MemberProvider.GetMember(type!, _observableMethodName, BindingMemberType.Method, memberFlags);
            _unsubscriber = (member as IObservableBindingMemberInfo)?.TryObserve(source, this) ?? Default.Disposable;
        }

        #endregion
    }
}