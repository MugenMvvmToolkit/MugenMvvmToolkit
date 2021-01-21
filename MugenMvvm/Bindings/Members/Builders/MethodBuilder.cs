using System;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct MethodBuilder<TTarget, TReturn> where TTarget : class?
    {
        private readonly Type _declaringType;
        private readonly string _name;
        private readonly Type _returnType;
        private MemberAttachedDelegate<IMethodMemberInfo, TTarget>? _attachedHandler;
        private TryObserveDelegate<INotifiableMemberInfo, TTarget>? _tryObserve;
        private RaiseDelegate<INotifiableMemberInfo, TTarget>? _raise;
        private object? _underlyingMember;
        private bool _isStatic;
        private bool _isObservable;
        private bool _isNonObservable;
        private Func<IMethodMemberInfo, ItemOrIReadOnlyList<IParameterInfo>>? _getParameters;
        private TryGetAccessorDelegate<IMethodMemberInfo>? _tryGetAccessor;
        private InvokeMethodDelegate<IMethodMemberInfo, TTarget, TReturn>? _invoke;

        public MethodBuilder(string name, Type declaringType, Type returnType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(returnType, nameof(returnType));
            _name = name;
            _declaringType = declaringType;
            _attachedHandler = null;
            _tryObserve = null;
            _raise = null;
            _isStatic = false;
            _isObservable = false;
            _isNonObservable = false;
            _returnType = returnType;
            _underlyingMember = null;
            _getParameters = null;
            _invoke = null;
            _tryGetAccessor = null;
        }

        public MethodBuilder<TTarget, TReturn> Static()
        {
            _isStatic = true;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> UnderlyingMember(object member)
        {
            Should.NotBeNull(member, nameof(member));
            _underlyingMember = member;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> AttachedHandler(MemberAttachedDelegate<IMethodMemberInfo, TTarget> attachedHandler)
        {
            Should.NotBeNull(attachedHandler, nameof(attachedHandler));
            _attachedHandler = attachedHandler;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> Observable()
        {
            Should.BeSupported(_tryObserve == null, nameof(ObservableHandler));
            _isObservable = true;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> Observable(IObservableMemberInfo? memberInfo)
        {
            if (memberInfo == null)
                return this;
            return ObservableHandler(memberInfo.TryObserve,
                memberInfo is INotifiableMemberInfo notifiableMember ? notifiableMember.Raise : (RaiseDelegate<IObservableMemberInfo, TTarget>?) null);
        }

        public MethodBuilder<TTarget, TReturn> ObservableHandler(TryObserveDelegate<IObservableMemberInfo, TTarget> tryObserve,
            RaiseDelegate<IObservableMemberInfo, TTarget>? raise = null)
        {
            Should.NotBeNull(tryObserve, nameof(tryObserve));
            Should.BeSupported(!_isObservable, nameof(Observable));
            _tryObserve = tryObserve;
            _raise = raise;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> NonObservable()
        {
            _isNonObservable = true;
            _isObservable = false;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> WithParameters(IParameterInfo parameters) => WithParameters(ItemOrIReadOnlyList.FromItem(parameters));

        public MethodBuilder<TTarget, TReturn> WithParameters(ItemOrIReadOnlyList<IParameterInfo> parameters)
        {
            var rawValue = parameters.GetRawValue();
            if (rawValue == null)
                return this;
            _getParameters = _ => ItemOrIReadOnlyList.FromRawValue<IParameterInfo>(rawValue);
            return this;
        }

        public MethodBuilder<TTarget, TReturn> GetParametersHandler(Func<IMethodMemberInfo, ItemOrIReadOnlyList<IParameterInfo>> getParameters)
        {
            Should.NotBeNull(getParameters, nameof(getParameters));
            _getParameters = getParameters;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> TryGetAccessorHandler(TryGetAccessorDelegate<IMethodMemberInfo> tryGetAccessor)
        {
            Should.NotBeNull(tryGetAccessor, nameof(tryGetAccessor));
            _tryGetAccessor = tryGetAccessor;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> InvokeHandler(InvokeMethodDelegate<IMethodMemberInfo, TTarget, TReturn> invoke)
        {
            Should.NotBeNull(invoke, nameof(invoke));
            _invoke = invoke;
            return this;
        }

        public IMethodMemberInfo Build()
        {
            Should.NotBeNull(_invoke, nameof(InvokeHandler));
            var id = _isObservable ? GenerateMemberId(true) : null;
            if (_attachedHandler == null)
            {
                if (!_isObservable)
                    return Method<object?>(null, _invoke, _getParameters, _tryObserve, _raise);

                return Method(id!, _invoke, _getParameters,
                    (member, target, listener, metadata) => EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State).Add(listener),
                    (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State, message, metadata));
            }

            RaiseDelegate<DelegateObservableMemberInfo<TTarget, (InvokeMethodDelegate<IMethodMemberInfo, TTarget, TReturn> _invoke,
                TryObserveDelegate<INotifiableMemberInfo, TTarget>? _tryObserve,
                MemberAttachedDelegate<IMethodMemberInfo, TTarget> _attachedHandler, string attachedId, string? id)>, TTarget>? raise = null;
            if (id != null)
                raise = (member, target, message, metadata) => EventListenerCollection.Raise(member.GetTarget(target), member.State.id!, message, metadata);
            var attachedId = GenerateMemberId(false);
            return Method((_invoke, _tryObserve, _attachedHandler, attachedId, id), (member, target, args, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, member, member.State._attachedHandler!, metadata);
                return member.State._invoke!(member, target, args, metadata);
            }, _getParameters, (member, target, listener, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, (IMethodMemberInfo) member, member.State._attachedHandler, metadata);
                if (member.State.id == null)
                    return member.State._tryObserve!(member, target, listener, metadata);
                return EventListenerCollection.GetOrAdd(member.GetTarget(target), member.State.id).Add(listener);
            }, raise ?? _raise);
        }

        private string GenerateMemberId(bool isMethodId) =>
            AttachedMemberBuilder.GenerateMemberId(isMethodId ? BindingInternalConstant.AttachedMethodPrefix : BindingInternalConstant.AttachedHandlerMethodPrefix, _declaringType,
                _name);

        private DelegateMethodMemberInfo<TTarget, TReturn, TState> Method<TState>(in TState state,
            InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturn, TState>, TTarget, TReturn> invoke,
            Func<DelegateMethodMemberInfo<TTarget, TReturn, TState>, ItemOrIReadOnlyList<IParameterInfo>>? getParameters,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) =>
            new(_name, _declaringType, _returnType, AttachedMemberBuilder.GetFlags(_isStatic, _isNonObservable), _underlyingMember,
                state, invoke, getParameters, _tryGetAccessor, _tryObserve == null && !_isObservable ? null : tryObserve, raise);
    }
}