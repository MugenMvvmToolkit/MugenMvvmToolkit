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
    public sealed class FieldMemberAccessorInfo : IMemberAccessorInfo
    {
        #region Fields

        private readonly FieldInfo _fieldInfo;
        private readonly IObserverProvider? _observerProvider;
        private readonly Type _reflectedType;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private Func<object?, object?> _getterFunc;

        private MemberObserver? _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public FieldMemberAccessorInfo(string name, FieldInfo fieldInfo, Type reflectedType, IObserverProvider? observerProvider,
            IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(fieldInfo, nameof(fieldInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _fieldInfo = fieldInfo;
            _reflectedType = reflectedType;
            _observerProvider = observerProvider;
            _reflectionDelegateProvider = reflectionDelegateProvider;
            Name = name;
            _getterFunc = CompileGetter;
            _setterFunc = CompileSetter;
            if (fieldInfo.IsStatic)
                AccessModifiers = fieldInfo.IsPublic ? MemberFlags.StaticPublic : MemberFlags.StaticNonPublic;
            else
                AccessModifiers = fieldInfo.IsPublic ? MemberFlags.InstancePublic : MemberFlags.InstanceNonPublic;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _fieldInfo.DeclaringType;

        public Type Type => _fieldInfo.FieldType;

        public object? Member => _fieldInfo;

        public MemberType MemberType => MemberType.Field;

        public MemberFlags AccessModifiers { get; }

        public bool CanRead => true;

        public bool CanWrite => true;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observerProvider.ServiceIfNull().TryGetMemberObserver(_reflectedType, _fieldInfo);
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

        private void CompileSetter(object? arg1, object? arg2)
        {
            _setterFunc = _fieldInfo.GetMemberSetter<object?>(_reflectionDelegateProvider);
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _fieldInfo.GetMemberGetter<object?>(_reflectionDelegateProvider);
            return _getterFunc(arg);
        }

        #endregion
    }
}