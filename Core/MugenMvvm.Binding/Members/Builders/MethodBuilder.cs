using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Observers;

namespace MugenMvvm.Binding.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct MethodBuilder<TTarget, TReturn> where TTarget : class?
    {
        #region Fields

        private readonly Type _declaringType;
        private readonly string _name;
        private readonly Type _returnType;
        private MemberAttachedDelegate<IMethodMemberInfo, TTarget>? _attachedHandler;
        private TryObserveDelegate<INotifiableMemberInfo, TTarget>? _tryObserve;
        private RaiseDelegate<INotifiableMemberInfo, TTarget>? _raise;
        private object? _underlyingMember;
        private bool _isStatic;
        private bool _isObservable;
        private Func<IMethodMemberInfo, IReadOnlyList<IParameterInfo>>? _getParameters;
        private InvokeMethodDelegate<IMethodMemberInfo, TTarget, TReturn>? _invoke;

        #endregion

        #region Constructors

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
            _returnType = returnType ?? typeof(EventHandler);
            _underlyingMember = null;
            _getParameters = null;
            _invoke = null;
        }

        #endregion

        #region Methods

        public MethodBuilder<TTarget, TReturn> Static()
        {
            Should.BeSupported(_attachedHandler == null, nameof(AttachedHandler));
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
            Should.BeSupported(!_isStatic, nameof(Static));
            _attachedHandler = attachedHandler;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> Observable()
        {
            Should.BeSupported(_tryObserve == null, nameof(ObservableHandler));
            _isObservable = true;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> ObservableHandler(TryObserveDelegate<IObservableMemberInfo, TTarget> tryObserve, RaiseDelegate<IObservableMemberInfo, TTarget>? raise = null)
        {
            Should.NotBeNull(tryObserve, nameof(tryObserve));
            Should.BeSupported(!_isObservable, nameof(Observable));
            _tryObserve = tryObserve;
            _raise = raise;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> WithParameters(params IParameterInfo[] parameters)
        {
            Should.NotBeNull(parameters, nameof(parameters));
            _getParameters = info => parameters;
            return this;
        }

        public MethodBuilder<TTarget, TReturn> GetParametersHandler(Func<IMethodMemberInfo, IReadOnlyList<IParameterInfo>> getParameters)
        {
            Should.NotBeNull(getParameters, nameof(getParameters));
            _getParameters = getParameters;
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

                if (_isStatic)
                {
                    return Method(id!, _invoke, _getParameters, (member, target, listener, metadata) => AttachedMemberBuilder.AddStaticEvent(member.State, listener),
                        (member, target, message, metadata) => AttachedMemberBuilder.RaiseStaticEvent(member.State, message, metadata));
                }

                return Method(id!, _invoke, _getParameters, (member, target, listener, metadata) => EventListenerCollection.GetOrAdd(target!, member.State).Add(listener),
                    (member, target, message, metadata) => EventListenerCollection.Raise(target!, member.State, message, metadata));
            }

            RaiseDelegate<DelegateObservableMemberInfo<TTarget, (InvokeMethodDelegate<IMethodMemberInfo, TTarget, TReturn> _invoke, TryObserveDelegate<INotifiableMemberInfo, TTarget>? _tryObserve,
                MemberAttachedDelegate<IMethodMemberInfo, TTarget> _attachedHandler, string attachedId, string? id)>, TTarget>? raise = null;
            if (id != null)
                raise = (member, target, message, metadata) => EventListenerCollection.Raise(target!, member.State.id!, message, metadata);
            var attachedId = GenerateMemberId(false);
            return Method((_invoke, _tryObserve, _attachedHandler, attachedId, id), (member, target, args, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, member, member.State._attachedHandler!, metadata);
                return member.State._invoke!(member, target, args, metadata);
            }, _getParameters, (member, target, listener, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedId, target, (IMethodMemberInfo)member, member.State._attachedHandler, metadata);
                if (member.State.id == null)
                    return member.State._tryObserve!(member, target, listener, metadata);
                return EventListenerCollection.GetOrAdd(target!, member.State.id).Add(listener);
            }, raise ?? _raise);
        }

        private string GenerateMemberId(bool isMethodId)
        {
            return AttachedMemberBuilder.GenerateMemberId(isMethodId ? BindingInternalConstant.AttachedMethodPrefix : BindingInternalConstant.AttachedHandlerMethodPrefix, _declaringType, _name);
        }

        private DelegateMethodMemberInfo<TTarget, TReturn, TState> Method<TState>(in TState state, InvokeMethodDelegate<DelegateMethodMemberInfo<TTarget, TReturn, TState>, TTarget, TReturn> invoke,
            Func<DelegateMethodMemberInfo<TTarget, TReturn, TState>, IReadOnlyList<IParameterInfo>>? getParameters, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
        {
            return new DelegateMethodMemberInfo<TTarget, TReturn, TState>(_name, _declaringType, _returnType, AttachedMemberBuilder.GetFlags(_isStatic), _underlyingMember,
                state, invoke, getParameters, _tryObserve == null && !_isObservable ? null : tryObserve, raise);
        }

        #endregion
    }
}