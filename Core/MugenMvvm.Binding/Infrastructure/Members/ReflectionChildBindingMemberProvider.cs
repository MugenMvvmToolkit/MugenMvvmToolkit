using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Members
{
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    public sealed class ReflectionChildBindingMemberProvider : IChildBindingMemberProvider
    {
        #region Fields

        private readonly IBindingObserverProvider _bindingObserverProvider;
        private readonly CacheDictionary _cache;

        private const MemberFlags DefaultFlags = MemberFlags.StaticPublic | MemberFlags.InstancePublic;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionChildBindingMemberProvider(IBindingObserverProvider bindingObserverProvider)
        {
            Should.NotBeNull(bindingObserverProvider, nameof(bindingObserverProvider));
            _bindingObserverProvider = bindingObserverProvider;
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags FieldMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags PropertyMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags EventMemberFlags { get; set; } = DefaultFlags;

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? TryGetMember(IBindingMemberProvider provider, Type type, string name, IReadOnlyMetadataContext metadata)
        {
            var cacheKey = new CacheKey(type, name);
            if (!_cache.TryGetValue(cacheKey, out var info))
            {
                info = GetMemberInternal(type, name, metadata);
                _cache[cacheKey] = info;
            }

            return info;
        }

        #endregion

        #region Methods

        private IBindingMemberInfo GetMemberInternal(Type type, string name, IReadOnlyMetadataContext metadata)
        {
            var indexerArgs = BindingMugenExtensions.GetIndexerValuesRaw(name);
            var types = BindingMugenExtensions.SelfAndBaseTypes(type);
            foreach (var t in types)
            {
                if (indexerArgs == null)
                {
                    var property = t.GetPropertyUnified(name, PropertyMemberFlags);
                    if (property != null)
                    {
                        return new PropertyInfoBindingMemberInfo(name, property, null, null, PropertyMemberFlags.HasMemberFlag(MemberFlags.NonPublic),
                            GetMemberObserver(type, name, metadata));
                    }
                }
                else
                {
                    PropertyInfo? candidate = null;
                    var valueTypeCount = -1;
                    ParameterInfo[]? indexParameters = null;
                    foreach (var property in t.GetPropertiesUnified(PropertyMemberFlags))
                    {
                        indexParameters = property.GetIndexParameters();
                        if (indexParameters.Length != indexerArgs.Length)
                            continue;
                        try
                        {
                            var count = 0;
                            for (var i = 0; i < indexParameters.Length; i++)
                            {
                                var arg = indexerArgs[i];
                                var paramType = indexParameters[i].ParameterType;
                                if (arg.StartsWith("\"", StringComparison.Ordinal) && arg.EndsWith("\"", StringComparison.Ordinal))
                                {
                                    if (paramType != typeof(string))
                                        break;
                                }
                                else
                                {
                                    //BindingServiceProvider.ValueConverter(Empty, paramType, arg);//todo converter
                                    if (paramType.IsValueTypeUnified())
                                        count++;
                                }
                            }

                            if (valueTypeCount < count)
                            {
                                candidate = property;
                                valueTypeCount = count;
                            }
                        }
                        catch
                        {
                            ;
                        }
                    }

                    if (candidate != null)
                    {
                        return new PropertyInfoBindingMemberInfo(name, candidate, indexParameters, indexerArgs, PropertyMemberFlags.HasMemberFlag(MemberFlags.NonPublic),
                            GetMemberObserver(type, name, metadata));
                    }

                    if (t.IsArray && t.GetArrayRank() == indexerArgs.Length)
                        return new ArrayBindingMemberInfo(name, t, indexerArgs);
                }

                var eventInfo = t.GetEventUnified(name, EventMemberFlags);
                if (eventInfo != null)
                    return new EventInfoBindingMemberInfo(name, eventInfo, GetMemberObserver(type, eventInfo, metadata));

                var field = t.GetFieldUnified(name, FieldMemberFlags);
                if (field != null)
                    return new FieldInfoBindingMemberInfo(name, field, GetMemberObserver(type, name, metadata));
            }

            return null;
        }

        private BindingMemberObserver? GetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata)
        {
            if (_bindingObserverProvider.TryGetMemberObserver(type, member, metadata, out var observer))
                return observer;
            return null;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct CacheKey
        {
            #region Fields

            public string Name;

            public Type Type;

            #endregion

            #region Constructors

            public CacheKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            #endregion
        }

        private sealed class CacheDictionary : LightDictionaryBase<CacheKey, IBindingMemberInfo>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Type.EqualsEx(y.Type) && string.Equals(x.Name, y.Name);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return key.Type.GetHashCode() * 397 ^ key.Name.GetHashCode();
                }
            }

            #endregion
        }

        private sealed class ArrayBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly int[] _indexes;

            #endregion

            #region Constructors

            public ArrayBindingMemberInfo(string name, Type arrayType, string[] indexes)
            {
                _indexes = BindingMugenExtensions.GetIndexerValues<int>(indexes);
                Name = name;
                Type = arrayType.GetElementType();
            }

            #endregion

            #region Properties

            public string Name { get; }

            public Type Type { get; }

            public object? Member => null;

            public BindingMemberType MemberType => BindingMemberType.Array;

            public bool CanRead => true;

            public bool CanWrite => true;

            public bool CanObserve => false;

            #endregion

            #region Implementation of interfaces

            public object GetValue(object target, object[] args)
            {
                return ((Array) target).GetValue(_indexes);
            }

            public object SetValue(object target, object value)
            {
                ((Array) target).SetValue(value, _indexes);
                return null;
            }

            public object SetValues(object target, object[] args)
            {
                return SetValue(target, args[0]);
            }

            public IDisposable TryObserve(object target, IBindingEventListener listener)
            {
                return null;
            }

            #endregion
        }

        private sealed class EventInfoBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly EventInfo _eventInfo;
            private readonly BindingMemberObserver? _observer;

            #endregion

            #region Constructors

            public EventInfoBindingMemberInfo(string name, EventInfo eventInfo, BindingMemberObserver? observer)
            {
                _eventInfo = eventInfo;
                _observer = observer;
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

            public object GetValue(object target, object[] args)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                return null;
            }

            public object SetValue(object target, object value)
            {
                var listener = value as IBindingEventListener;
                if (_observer == null || listener == null)
                    BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);

                return _observer!.Value.TryObserve(target, listener, Default.Metadata);
            }

            public object SetValues(object target, object[] args)
            {
                return SetValue(target, args[0]);
            }

            public IDisposable TryObserve(object target, IBindingEventListener listener)
            {
                return null;
            }

            #endregion
        }

        private sealed class FieldInfoBindingMemberInfo : IBindingMemberInfo
        {
            #region Fields

            private readonly FieldInfo _fieldInfo;
            private readonly BindingMemberObserver? _observer;
            private Func<object, object>? _getterFunc;
            private Action<object, object>? _setterFunc;

            #endregion

            #region Constructors

            public FieldInfoBindingMemberInfo(string name, FieldInfo fieldInfo, BindingMemberObserver? observer)
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

            public object GetValue(object target, object[] args)
            {
                return _getterFunc(target);
            }

            public object SetValue(object target, object value)
            {
                _setterFunc(target, value);
                return null;
            }

            public object SetValues(object target, object[] args)
            {
                return SetValue(target, args[0]);
            }

            public IDisposable TryObserve(object target, IBindingEventListener listener)
            {
                return _observer?.TryObserve(target, listener, Default.Metadata);
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
            private readonly BindingMemberObserver? _observer;
            private readonly PropertyInfo _propertyInfo;

            private Func<object, object>? _getterFunc;
            private Func<object, object[], object>? _getterIndexerFunc;

            private Action<object, object>? _setterFunc;
            private Func<object, object[], object>? _setterIndexerFunc;

            #endregion

            #region Constructors

            public PropertyInfoBindingMemberInfo(string name, PropertyInfo propertyInfo, ParameterInfo[] indexParameters, string[]? indexerValues, bool nonPublic,
                BindingMemberObserver? observer)
            {
                _propertyInfo = propertyInfo;
                _nonPublic = nonPublic;
                _observer = observer;
                Name = name;
                Type = _propertyInfo.PropertyType;
                if (indexParameters != null)
                    _indexerValues = BindingMugenExtensions.GetIndexerValues(indexerValues, indexParameters);

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

            public object GetValue(object target, object[] args)
            {
                if (!CanRead)
                {
                    BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                    return null;
                }

                if (_getterIndexerFunc == null)
                    return _getterFunc(target);
                return _getterIndexerFunc(target, _indexerValues);
            }

            public object SetValue(object target, object value)
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
                    return _setterIndexerFunc(target, args);
                }

                _setterFunc(target, value);
                return null;
            }

            public object SetValues(object target, object[] args)
            {
                return SetValue(target, args[0]);
            }

            public IDisposable TryObserve(object target, IBindingEventListener listener)
            {
                return _observer?.TryObserve(target, listener, Default.Metadata);
            }

            #endregion

            #region Methods

            private object CompileIndexerSetter(object arg1, object[] arg2)
            {
                _setterIndexerFunc = _propertyInfo.GetSetMethodUnified(_nonPublic).GetMethodInvoker();
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
                _getterIndexerFunc = _propertyInfo.GetGetMethodUnified(_nonPublic).GetMethodInvoker();
                return _getterIndexerFunc(arg, values);
            }

            #endregion
        }

        #endregion
    }
}