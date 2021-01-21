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
        private readonly FieldInfo _fieldInfo;
        private readonly ushort _modifiers;
        private readonly Type _reflectedType;
        private Func<object?, object?> _getterFunc;
        private MemberObserver _observer;
        private Action<object?, object?> _setterFunc;

        public FieldAccessorMemberInfo(string name, FieldInfo fieldInfo, Type reflectedType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(fieldInfo, nameof(fieldInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _fieldInfo = fieldInfo;
            _reflectedType = reflectedType;
            Name = name;
            _getterFunc = CompileGetter;
            _modifiers = fieldInfo.GetAccessModifiers().Value();
            if (fieldInfo.IsInitOnly)
            {
                _setterFunc = MustBeWritable;
                _modifiers |= Enums.MemberFlags.NonObservable.Value;
            }
            else
                _setterFunc = CompileSetter;
        }

        public bool CanRead => true;

        public bool CanWrite => !_fieldInfo.IsInitOnly;

        public string Name { get; }

        public Type DeclaringType => _fieldInfo.DeclaringType ?? typeof(object);

        public Type Type => _fieldInfo.FieldType;

        public object UnderlyingMember => _fieldInfo;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> MemberFlags => new(_modifiers);

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null) => _getterFunc(target);

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null) => _setterFunc(target, value);

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            if (_observer.IsEmpty)
                _observer = MugenService.ObservationManager.TryGetMemberObserver(_reflectedType, this, metadata).NoDoIfEmpty();
            return _observer.TryObserve(target, listener, metadata);
        }

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

        private void MustBeWritable(object? _, object? __) => ExceptionManager.ThrowBindingMemberMustBeWritable(this);
    }
}