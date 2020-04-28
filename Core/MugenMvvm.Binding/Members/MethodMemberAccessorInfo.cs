using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class MethodMemberAccessorInfo : IMemberAccessorInfo, IHasArgsMemberInfo
    {
        #region Fields

        private readonly object?[] _args;
        private readonly IMethodInfo? _getMethod;
        private readonly IObserverProvider? _observerProvider;
        private readonly Type _reflectedType;
        private readonly IMethodInfo? _setMethod;
        private MemberObserver? _observer;

        #endregion

        #region Constructors

        public MethodMemberAccessorInfo(string name, IMethodInfo? getMethod, IMethodInfo? setMethod, object?[] args, ArgumentFlags argumentFlags, Type reflectedType, IObserverProvider? observerProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(args, nameof(args));
            if (getMethod == null)
                Should.NotBeNull(setMethod, nameof(setMethod));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            Name = name;
            Type = getMethod?.Type ?? setMethod!.GetParameters().Last().ParameterType;
            _getMethod = getMethod;
            _setMethod = setMethod;
            _args = args;
            ArgumentFlags = argumentFlags;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
        }

        #endregion

        #region Properties

        public ArgumentFlags ArgumentFlags { get; }

        public string Name { get; }

        public Type DeclaringType => _getMethod?.DeclaringType ?? _setMethod!.DeclaringType;

        public Type Type { get; }

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Accessor;

        public MemberFlags AccessModifiers => _getMethod?.AccessModifiers ?? _setMethod!.AccessModifiers;

        public bool CanRead => _getMethod != null;

        public bool CanWrite => _setMethod != null;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<object?> GetArgs()
        {
            return _args;
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
            {
                _observer = _observerProvider
                    .DefaultIfNull()
                    .GetMemberObserver(_reflectedType, new MemberObserverRequest(Name, (_getMethod?.UnderlyingMember ?? _setMethod?.UnderlyingMember) as MemberInfo, _args, this), metadata);
            }

            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            if (_getMethod == null)
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            object?[] args;
            if (ArgumentFlags.HasFlagEx(ArgumentFlags.Metadata))
            {
                args = new object?[_args.Length];
                Array.Copy(_args, args, _args.Length);
                args[args.Length - 1] = metadata;
            }
            else
                args = _args;

            return _getMethod.Invoke(target, args, metadata);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (_setMethod == null)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            var args = new object?[_args.Length + 1];
            Array.Copy(_args, args, _args.Length);
            args[args.Length - 1] = value;
            if (ArgumentFlags.HasFlagEx(ArgumentFlags.Metadata))
                args[args.Length - 2] = metadata;
            _setMethod.Invoke(target, args, metadata);
        }

        #endregion
    }
}