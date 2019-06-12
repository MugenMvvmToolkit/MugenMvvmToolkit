using System;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    public sealed class ReflectionChildBindingMemberProvider : IChildBindingMemberProvider
    {
        #region Fields

        private const MemberFlags DefaultFlags = MemberFlags.StaticPublic | MemberFlags.InstancePublic;

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags FieldMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags PropertyMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags EventMemberFlags { get; set; } = DefaultFlags;

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo GetMember(IBindingMemberProvider provider, Type type, string name, IReadOnlyMetadataContext metadata)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nested types

        private sealed class EventInfoBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly EventInfo _eventInfo;

            #endregion

            #region Constructors

            public EventInfoBindingMemberInfo(string name, EventInfo eventInfo)
            {
                _eventInfo = eventInfo;
                Name = name;
                Type = _eventInfo.EventHandlerType;
            }

            #endregion

            #region Properties

            public string Name { get; }

            public Type Type { get; }

            public object Member => _eventInfo;

            public BindingMemberType MemberType => BindingMemberType.Event;

            public bool CanRead => false;

            public bool CanWrite => true;

            public bool CanObserve => false;

            #endregion

            #region Implementation of interfaces

            public object GetValue(object source, object[] args)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                return null;
            }

            public object SetValue(object source, object value)
            {
                var listener = value as IBindingEventListener;
                if (listener == null)
                    BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);

                return null;
                //                return BindingServiceProvider.WeakEventManager.TrySubscribe(o, (EventInfo)_member, listener);
            }

            public object SetValues(object source, object[] args)
            {
                return SetValue(source, args[0]);
            }

            public IDisposable TryObserve(object source, IBindingEventListener listener)
            {
                return null; //todo observer provider method?
            }

            #endregion
        }

        private sealed class FieldInfoBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly FieldInfo _fieldInfo;
            private readonly IBindingMemberObserver? _observer;
            private Func<object, object>? _getterFunc;
            private Action<object, object>? _setterFunc;

            #endregion

            #region Constructors

            public FieldInfoBindingMemberInfo(string name, FieldInfo fieldInfo, IBindingMemberObserver? observer)
            {
                _fieldInfo = fieldInfo;
                _observer = observer;
                Name = name;
                Type = _fieldInfo.FieldType;
                _getterFunc = CompileGetter;
                _setterFunc = CompileSetter;
            }

            #endregion

            #region Properties

            public string Name { get; }

            public Type Type { get; }

            public object Member => _fieldInfo;

            public BindingMemberType MemberType => BindingMemberType.Field;

            public bool CanRead => true;

            public bool CanWrite => true;

            public bool CanObserve => _observer != null;

            #endregion

            #region Implementation of interfaces

            public object GetValue(object source, object[] args)
            {
                return _getterFunc(source);
            }

            public object SetValue(object source, object value)
            {
                _setterFunc(source, value);
                return null;
            }

            public object SetValues(object source, object[] args)
            {
                return SetValue(source, args[0]);
            }

            public IDisposable TryObserve(object source, IBindingEventListener listener)
            {
                return _observer?.TryObserve(source, Name, listener, Default.Metadata);
            }

            #endregion

            #region Methods

            private void CompileSetter(object arg1, object arg2)
            {
                _setterFunc = _fieldInfo.GetMemberSetter<object>();
                _setterFunc(arg1, arg2);
            }

            private object CompileGetter(object arg)
            {
                _getterFunc = _fieldInfo.GetMemberGetter<object>();
                return _getterFunc(arg);
            }

            #endregion
        }

        private sealed class PropertyInfoBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly object[]? _indexerValues;

            private readonly bool _nonPublic;
            private readonly IBindingMemberObserver? _observer;
            private readonly PropertyInfo _propertyInfo;

            private Func<object, object>? _getterFunc;
            private Func<object, object[], object>? _getterIndexerFunc;

            private Action<object, object>? _setterFunc;
            private Func<object, object[], object>? _setterIndexerFunc;

            #endregion

            #region Constructors

            public PropertyInfoBindingMemberInfo(string name, PropertyInfo propertyInfo, bool nonPublic, IBindingMemberObserver? observer)
            {
                _propertyInfo = propertyInfo;
                _nonPublic = nonPublic;
                _observer = observer;
                Name = name;
                Type = _propertyInfo.PropertyType;
                var indexParameters = propertyInfo.GetIndexParameters();
                if (indexParameters.Length != 0)
                    _indexerValues = BindingMugenExtensions.GetIndexerValues(name);

                var method = propertyInfo.GetGetMethodUnified(nonPublic);
                if (method == null)
                    CanRead = false;
                else
                {
                    CanRead = true;
                    if (_indexerValues == null)
                        _getterFunc = CompileGetter;
                    else
                        _getterIndexerFunc = CompileIndexerGetter;
                }

                method = propertyInfo.GetSetMethodUnified(nonPublic);
                if (method == null)
                    CanWrite = false;
                else
                {
                    CanWrite = true;
                    if (_indexerValues == null)
                        _setterFunc = CompileSetter;
                    else
                        _setterIndexerFunc = CompileIndexerSetter;
                }
            }

            #endregion

            #region Properties

            public string Name { get; }

            public Type Type { get; }

            public object Member => _propertyInfo;

            public BindingMemberType MemberType => BindingMemberType.Property;

            public bool CanRead { get; }

            public bool CanWrite { get; }

            public bool CanObserve => _observer != null;

            #endregion

            #region Implementation of interfaces

            public object GetValue(object source, object[] args)
            {
                if (!CanRead)
                {
                    BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                    return null;
                }

                if (_getterIndexerFunc == null)
                    return _getterFunc(source);
                return _getterIndexerFunc(source, _indexerValues);
            }

            public object SetValue(object source, object value)
            {
                if (!CanWrite)
                {
                    BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
                    return null;
                }

                if (_setterFunc == null)
                {
                    var args = new object[_indexerValues.Length + 1];
                    Array.Copy(_indexerValues, args, _indexerValues.Length);
                    args[_indexerValues.Length] = value;
                    return _setterIndexerFunc(source, args);
                }

                _setterFunc(source, value);
                return null;
            }

            public object SetValues(object source, object[] args)
            {
                return SetValue(source, args[0]);
            }

            public IDisposable TryObserve(object source, IBindingEventListener listener)
            {
                return _observer?.TryObserve(source, Name, listener, Default.Metadata);
            }

            #endregion

            #region Methods

            private object CompileIndexerSetter(object arg1, object[] arg2)
            {
                _setterIndexerFunc = _propertyInfo.GetSetMethodUnified(_nonPublic).GetMethodDelegate();
                return _setterIndexerFunc(arg1, arg2);
            }

            private void CompileSetter(object arg1, object arg2)
            {
                _setterFunc = _propertyInfo.GetMemberSetter<object>();
                _setterFunc(arg1, arg2);
            }

            private object CompileGetter(object arg)
            {
                _getterFunc = _propertyInfo.GetMemberGetter<object>();
                return _getterFunc(arg);
            }

            private object CompileIndexerGetter(object arg, object[] values)
            {
                _getterIndexerFunc = _propertyInfo.GetGetMethodUnified(_nonPublic).GetMethodDelegate();
                return _getterIndexerFunc(arg, values);
            }

            #endregion
        }

        #endregion
    }
}