using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class MethodAccessorMemberInfo : IAccessorMemberInfo, IHasArgsMemberInfo
    {
        #region Fields

        private readonly ItemOrArray<object?> _args;
        private readonly IMethodMemberInfo? _getMethod;
        private readonly IObservationManager? _observationManager;
        private readonly Type _reflectedType;
        private readonly IMethodMemberInfo? _setMethod;
        private MemberObserver _observer;

        #endregion

        #region Constructors

        public MethodAccessorMemberInfo(string name, IMethodMemberInfo? getMethod, IMethodMemberInfo? setMethod, ItemOrArray<object?> args, EnumFlags<ArgumentFlags> argumentFlags, Type reflectedType,
            IObservationManager? observationManager)
        {
            Should.NotBeNull(name, nameof(name));
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
            _observationManager = observationManager;
        }

        #endregion

        #region Properties

        public EnumFlags<ArgumentFlags> ArgumentFlags { get; }

        public string Name { get; }

        public Type DeclaringType => _getMethod?.DeclaringType ?? _setMethod!.DeclaringType;

        public Type Type { get; }

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> AccessModifiers => _getMethod?.AccessModifiers ?? _setMethod!.AccessModifiers;

        public bool CanRead => _getMethod != null;

        public bool CanWrite => _setMethod != null;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_getMethod != null)
            {
                var token = _getMethod.TryObserve(target, listener, metadata);
                if (!token.IsEmpty)
                    return token;
            }

            if (_observer.IsEmpty)
            {
                _observer = _observationManager
                    .DefaultIfNull()
                    .TryGetMemberObserver(_reflectedType, this, metadata)
                    .NoDoIfEmpty();
            }

            return _observer.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            if (_getMethod == null)
                ExceptionManager.ThrowBindingMemberMustBeReadable(this);
            ItemOrArray<object?> args;
            if (ArgumentFlags.HasFlag(Enums.ArgumentFlags.Metadata))
            {
                if (_args.Count == 1)
                    args = new ItemOrArray<object?>(metadata, true);
                else
                {
                    var argsArray = new object?[_args.List!.Length];
                    Array.Copy(_args.List!, argsArray, argsArray.Length);
                    argsArray[argsArray.Length - 1] = metadata;
                    args = argsArray;
                }
            }
            else
                args = _args;

            return _getMethod.Invoke(target, args, metadata);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (_setMethod == null)
                ExceptionManager.ThrowBindingMemberMustBeWritable(this);
            var args = new object?[_args.Count + 1];
            if (_args.HasItem)
                args[0] = _args.Item;
            else
                Array.Copy(_args.List!, args, _args.Count);
            args[args.Length - 1] = value;
            if (ArgumentFlags.HasFlag(Enums.ArgumentFlags.Metadata))
                args[args.Length - 2] = metadata;
            _setMethod.Invoke(target, args, metadata);
        }

        public ItemOrIReadOnlyList<object?> GetArgs() => _args;

        #endregion
    }
}