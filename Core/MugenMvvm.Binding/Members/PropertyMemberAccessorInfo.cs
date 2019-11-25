using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class PropertyMemberAccessorInfo : IMemberAccessorInfo
    {
        #region Fields

        private readonly IObserverProvider? _observerProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;

        private readonly PropertyInfo _propertyInfo;
        private readonly Type _reflectedType;
        private Func<object?, object?> _getterFunc;

        private MemberObserver? _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public PropertyMemberAccessorInfo(string name, PropertyInfo propertyInfo, Type reflectedType, IObserverProvider? observerProvider, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _propertyInfo = propertyInfo;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            Name = name;
            Type = _propertyInfo.PropertyType;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
            {
                CanRead = false;
                _getterFunc = MustBeReadable;
            }
            else
            {
                CanRead = true;
                _getterFunc = CompileGetter;
            }

            var setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
            {
                CanWrite = false;
                _setterFunc = MustBeWritable;
            }
            else
            {
                CanWrite = true;
                _setterFunc = CompileSetter;
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

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.DefaultIfNull().TryGetMemberObserver(_reflectedType, _propertyInfo);
            return _observer.Value.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            return _getterFunc(target);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            _setterFunc(target, value);
        }

        #endregion

        #region Methods

        private void MustBeWritable(object? _, object? __)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        private object MustBeReadable(object? _)
        {
            BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return null;
        }

        private void CompileSetter(object? arg1, object? arg2)
        {
            _setterFunc = _propertyInfo.GetMemberSetter<object?, object?>(_reflectionDelegateProvider);
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _propertyInfo.GetMemberGetter<object?, object?>(_reflectionDelegateProvider);
            return _getterFunc(arg);
        }

        #endregion
    }
}