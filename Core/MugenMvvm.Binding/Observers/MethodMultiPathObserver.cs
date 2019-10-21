﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MethodMultiPathObserver : MultiPathObserver
    {
        #region Fields

        private readonly string _observableMethodName;

        private IWeakReference? _lastValueRef;
        private Unsubscriber _unsubscriber;

        #endregion

        #region Constructors

        public MethodMultiPathObserver(string observableMethodName, object source, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(source, path, memberFlags, hasStablePath, true, optional)
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
            _unsubscriber.Unsubscribe();
            _unsubscriber = default;
        }

        private void AddMethodObserver(object? source, IBindingMemberInfo? lastMember)
        {
            _unsubscriber.Unsubscribe();
            if (source == null || !(lastMember is IBindingPropertyInfo propertyInfo))
            {
                _unsubscriber = Unsubscriber.NoDoUnsubscriber;
                return;
            }

            var value = propertyInfo.GetValue(source);
            if (ReferenceEquals(value, _lastValueRef?.Target))
                return;

            var type = value?.GetType()!;
            if (value.IsNullOrUnsetValue() || type.IsValueTypeUnified())
            {
                _unsubscriber = Unsubscriber.NoDoUnsubscriber;
                return;
            }

            _lastValueRef = value.ToWeakReference();
            var memberFlags = MemberFlags & ~MemberFlags.Static;
            var member = MugenBindingService.MemberProvider.GetMember(type!, _observableMethodName, BindingMemberType.Method, memberFlags);
            if (member is IObservableBindingMemberInfo observable)
                _unsubscriber = observable.TryObserve(source, GetLastMemberListener());
            if (_unsubscriber.IsEmpty)
                _unsubscriber = Unsubscriber.NoDoUnsubscriber;
        }

        #endregion
    }
}