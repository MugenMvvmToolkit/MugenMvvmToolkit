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
    public sealed class PropertyAccessorMemberInfo : IAccessorMemberInfo
    {
        #region Fields

        private readonly ushort _modifiers;

        private readonly PropertyInfo _propertyInfo;
        private readonly Type _reflectedType;
        private Func<object?, object?> _getterFunc;

        private MemberObserver _observer;
        private Action<object?, object?> _setterFunc;

        #endregion

        #region Constructors

        public PropertyAccessorMemberInfo(string name, PropertyInfo propertyInfo, Type reflectedType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(propertyInfo, nameof(propertyInfo));
            Should.NotBeNull(reflectedType, nameof(reflectedType));
            _propertyInfo = propertyInfo;
            _reflectedType = reflectedType;
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

            _modifiers = (getMethod ?? setMethod).GetAccessModifiers().Value();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => _propertyInfo.DeclaringType ?? typeof(object);

        public Type Type { get; }

        public object UnderlyingMember => _propertyInfo;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> AccessModifiers => new(_modifiers);

        public bool CanRead { get; }

        public bool CanWrite { get; }

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

        private void MustBeWritable(object? _, object? __) => ExceptionManager.ThrowBindingMemberMustBeWritable(this);

        private object MustBeReadable(object? _)
        {
            ExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return null;
        }

        private void CompileSetter(object? arg1, object? arg2)
        {
            _setterFunc = _propertyInfo.GetMemberSetter<object?, object?>();
            _setterFunc(arg1, arg2);
        }

        private object? CompileGetter(object? arg)
        {
            _getterFunc = _propertyInfo.GetMemberGetter<object?, object?>();
            return _getterFunc(arg);
        }

        #endregion
    }
}