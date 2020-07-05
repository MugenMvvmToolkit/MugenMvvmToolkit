using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public sealed class FieldAccessorMemberInfo : IAccessorMemberInfo
    {
        #region Fields

        private readonly FieldInfo _fieldInfo;
        private readonly IObservationManager? _observationManager;
        private readonly Type _reflectedType;
        private readonly IReflectionManager? _reflectionManager;
        private Func<object?, object?> _getterFunc;

        private MemberObserver? _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public FieldAccessorMemberInfo(string name, FieldInfo fieldInfo, Type reflectedType, IObservationManager? observationManager,
            IReflectionManager? reflectionManager)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(fieldInfo, nameof(fieldInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _fieldInfo = fieldInfo;
            _reflectedType = reflectedType;
            _observationManager = observationManager;
            _reflectionManager = reflectionManager;
            Name = name;
            _getterFunc = CompileGetter;
            _setterFunc = CompileSetter;
            AccessModifiers = fieldInfo.GetAccessModifiers();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _fieldInfo.DeclaringType;

        public Type Type => _fieldInfo.FieldType;

        public object? UnderlyingMember => _fieldInfo;

        public MemberType MemberType => MemberType.Accessor;

        public MemberFlags AccessModifiers { get; }

        public bool CanRead => true;

        public bool CanWrite => true;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer == null)
                _observer = _observationManager.DefaultIfNull().TryGetMemberObserver(_reflectedType, this, metadata);
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
            _setterFunc = _fieldInfo.GetMemberSetter<object?, object?>(_reflectionManager);
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _fieldInfo.GetMemberGetter<object?, object?>(_reflectionManager);
            return _getterFunc(arg);
        }

        #endregion
    }
}