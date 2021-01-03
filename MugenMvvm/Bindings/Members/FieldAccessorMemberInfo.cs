using System;
using System.Reflection;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class FieldAccessorMemberInfo : IAccessorMemberInfo
    {
        #region Fields

        private readonly FieldInfo _fieldInfo;
        private readonly ushort _modifiers;
        private readonly Type _reflectedType;
        private Func<object?, object?> _getterFunc;
        private MemberObserver _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public FieldAccessorMemberInfo(string name, FieldInfo fieldInfo, Type reflectedType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(fieldInfo, nameof(fieldInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _fieldInfo = fieldInfo;
            _reflectedType = reflectedType;
            Name = name;
            _getterFunc = CompileGetter;
            _setterFunc = CompileSetter;
            _modifiers = fieldInfo.GetAccessModifiers().Value();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _fieldInfo.DeclaringType ?? typeof(object);

        public Type Type => _fieldInfo.FieldType;

        public object UnderlyingMember => _fieldInfo;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> AccessModifiers => new(_modifiers);

        public bool CanRead => true;

        public bool CanWrite => true;

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer.IsEmpty)
                _observer = MugenService.ObservationManager.TryGetMemberObserver(_reflectedType, this, metadata).NoDoIfEmpty();
            return _observer.TryObserve(target, listener, metadata);
        }

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null) => _getterFunc(target);

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null) => _setterFunc(target, value);

        #endregion

        #region Methods

        private void CompileSetter(object? arg1, object? arg2)
        {
            _setterFunc = _fieldInfo.GetMemberSetter<object?, object?>();
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _fieldInfo.GetMemberGetter<object?, object?>();
            return _getterFunc(arg);
        }

        #endregion
    }
}