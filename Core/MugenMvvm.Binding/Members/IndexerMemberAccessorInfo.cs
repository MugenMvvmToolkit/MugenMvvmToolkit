using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class IndexerMemberAccessorInfo : IMemberAccessorInfo, IHasArgsMemberInfo
    {
        #region Fields

        private readonly object?[] _indexerArgs;
        private readonly IObserverProvider? _observerProvider;

        private readonly PropertyInfo _propertyInfo;
        private readonly Type _reflectedType;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private Func<object?, object?[], object?> _getterIndexerFunc;

        private MemberObserver? _observer;
        private Func<object?, object?[], object?> _setterIndexerFunc;

        #endregion

        #region Constructors

        public IndexerMemberAccessorInfo(string name, PropertyInfo propertyInfo, object?[] indexerArgs,
            Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            Should.NotBeNull(indexerArgs, nameof(indexerArgs));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _propertyInfo = propertyInfo;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _indexerArgs = indexerArgs;
            Name = name;
            Type = _propertyInfo.PropertyType;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
            {
                CanRead = false;
                _getterIndexerFunc = MustBeReadable;
            }
            else
            {
                CanRead = true;
                _getterIndexerFunc = CompileIndexerGetter;
            }

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
            {
                CanWrite = false;
                _setterIndexerFunc = MustBeWritable;
            }
            else
            {
                CanWrite = true;
                _setterIndexerFunc = CompileIndexerSetter;
            }

            AccessModifiers = (getMethod ?? setMethod).GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _propertyInfo.DeclaringType;

        public Type Type { get; }

        public object? UnderlyingMember => _propertyInfo;

        public MemberType MemberType => MemberType.Accessor;

        public MemberFlags AccessModifiers { get; }

        public bool CanRead { get; }

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<object?> GetArgs()
        {
            return _indexerArgs;
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.DefaultIfNull().TryGetMemberObserver(_reflectedType, new MemberObserverRequest(Name, _propertyInfo, _indexerArgs, this));
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            return _getterIndexerFunc(target, _indexerArgs);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            var args = new object?[_indexerArgs.Length + 1];
            Array.Copy(_indexerArgs, args, _indexerArgs.Length);
            args[_indexerArgs.Length] = value;
            _setterIndexerFunc(target, args);
        }

        #endregion

        #region Methods

        private object? MustBeWritable(object? _, object? __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            return null;
        }

        private object? MustBeReadable(object? _, object?[] __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return null;
        }

        private object? CompileIndexerSetter(object? arg1, object?[] arg2)
        {
            _setterIndexerFunc = _propertyInfo.GetSetMethod(true)!.GetMethodInvoker(_reflectionDelegateProvider);
            return _setterIndexerFunc(arg1, arg2);
        }

        private object? CompileIndexerGetter(object? arg, object?[] values)
        {
            _getterIndexerFunc = _propertyInfo.GetGetMethod(true)!.GetMethodInvoker(_reflectionDelegateProvider);
            return _getterIndexerFunc(arg, values);
        }

        #endregion
    }
}