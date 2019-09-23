using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    public sealed class ReflectionMemberProviderComponent : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObserverProvider? _bindingObserverProvider;
        private readonly CacheDictionary _cache;

        private const MemberFlags DefaultFlags = MemberFlags.StaticPublic | MemberFlags.InstancePublic;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionMemberProviderComponent(IObserverProvider? bindingObserverProvider = null)
        {
            _bindingObserverProvider = bindingObserverProvider;
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags FieldMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags PropertyMemberFlags { get; set; } = DefaultFlags;

        public MemberFlags EventMemberFlags { get; set; } = DefaultFlags;

        private IObserverProvider ObserverProvider => _bindingObserverProvider.ServiceIfNull();

        #endregion

        #region Implementation of interfaces

        public IBindingMemberInfo? TryGetMember(Type type, string name, IReadOnlyMetadataContext? metadata)
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

        private IBindingMemberInfo? GetMemberInternal(Type type, string name, IReadOnlyMetadataContext? metadata)
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
                        return new BindingPropertyInfo(name, property, PropertyMemberFlags.HasMemberFlag(MemberFlags.NonPublic),
                            ObserverProvider.GetMemberObserver(type, name, metadata));
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

                        var count = 0;
                        for (var i = 0; i < indexParameters.Length; i++)
                        {
                            var arg = indexerArgs[i];
                            var paramType = indexParameters[i].ParameterType;
                            if (arg.StartsWith("\"", StringComparison.Ordinal) && arg.EndsWith("\"", StringComparison.Ordinal))
                            {
                                if (paramType != typeof(string))
                                {
                                    count = -1;
                                    break;
                                }
                            }
                            else
                            {
                                if (!MugenBindingService.GlobalValueConverter.TryConvert(arg, paramType, null, metadata, out _))
                                {
                                    count = -1;
                                    break;
                                }

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

                    if (candidate != null)
                        return new IndexerBindingPropertyInfo(name, candidate, indexParameters!, indexerArgs,
                            PropertyMemberFlags.HasMemberFlag(MemberFlags.NonPublic), ObserverProvider.GetMemberObserver(type, name, metadata));

                    if (t.IsArray && t.GetArrayRank() == indexerArgs.Length)
                        return new ArrayBindingMemberInfo(name, t, indexerArgs);
                }

                var eventInfo = t.GetEventUnified(name, EventMemberFlags);
                if (eventInfo != null)
                    return new BindingEventInfo(name, eventInfo, ObserverProvider.GetMemberObserver(type, eventInfo, metadata));

                var field = t.GetFieldUnified(name, FieldMemberFlags);
                if (field != null)
                    return new BindingFieldInfo(name, field, ObserverProvider.GetMemberObserver(type, name, metadata));
            }

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

        private sealed class CacheDictionary : LightDictionaryBase<CacheKey, IBindingMemberInfo?>
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

        private sealed class BindingFieldInfo : IBindingPropertyInfo
        {
            #region Fields

            private readonly FieldInfo _fieldInfo;
            private readonly MemberObserver _observer;
            private Func<object?, object?> _getterFunc;
            private Action<object?, object?> _setterFunc;

            #endregion

            #region Constructors

            public BindingFieldInfo(string name, FieldInfo fieldInfo, MemberObserver observer)
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

            public bool IsAttached => false;

            public string Name { get; }

            public Type Type { get; }

            public object? Member => _fieldInfo;

            public BindingMemberType MemberType => BindingMemberType.Property;

            public bool CanRead => true;

            public bool CanWrite => true;

            #endregion

            #region Implementation of interfaces

            public IDisposable? TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                return _observer.TryObserve(source, listener, metadata);
            }

            public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
            {
                return _getterFunc(source);
            }

            public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
            {
                _setterFunc(source, value);
            }

            #endregion

            #region Methods

            private void CompileSetter(object? arg1, object? arg2)
            {
                _setterFunc = _fieldInfo.GetMemberSetter<object?>();
                _setterFunc(arg1, arg2);
            }

            private object? CompileGetter(object? arg)
            {
                _getterFunc = _fieldInfo.GetMemberGetter<object?>();
                return _getterFunc(arg);
            }

            #endregion
        }

        private sealed class BindingPropertyInfo : IBindingPropertyInfo
        {
            #region Fields

            private readonly MemberObserver _observer;
            private readonly PropertyInfo _propertyInfo;

            private Func<object?, object?>? _getterFunc;
            private Action<object?, object?>? _setterFunc;

            #endregion

            #region Constructors

            public BindingPropertyInfo(string name, PropertyInfo propertyInfo, bool nonPublic, MemberObserver observer)
            {
                _propertyInfo = propertyInfo;
                _observer = observer;
                Name = name;
                Type = _propertyInfo.PropertyType;

                var method = propertyInfo.GetGetMethodUnified(nonPublic);
                if (method == null)
                {
                    CanRead = false;
                    _getterFunc = MustBeReadable;
                }
                else
                {
                    CanRead = true;
                    _getterFunc = CompileGetter;
                }

                method = propertyInfo.GetSetMethodUnified(nonPublic);
                if (method == null)
                {
                    CanWrite = false;
                    _setterFunc = MustBeWritable;
                }
                else
                {
                    CanWrite = true;
                    _setterFunc = CompileSetter;
                }
            }

            #endregion

            #region Properties

            public bool IsAttached => false;

            public string Name { get; }

            public Type Type { get; }

            public object? Member => _propertyInfo;

            public BindingMemberType MemberType => BindingMemberType.Property;

            public bool CanRead { get; }

            public bool CanWrite { get; }

            #endregion

            #region Implementation of interfaces

            public IDisposable? TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                return _observer.TryObserve(source, listener, metadata);
            }

            public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
            {
                return _getterFunc!(source);
            }

            public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
            {
                _setterFunc(source, value);
            }

            #endregion

            #region Methods

            private void MustBeWritable(object _, object __)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            }

            private object MustBeReadable(object _)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                return null;
            }

            private void CompileSetter(object? arg1, object? arg2)
            {
                _setterFunc = _propertyInfo.GetMemberSetter<object?>();
                _setterFunc(arg1, arg2);
            }

            private object? CompileGetter(object? arg)
            {
                _getterFunc = _propertyInfo.GetMemberGetter<object>();
                return _getterFunc(arg);
            }

            #endregion
        }

        private sealed class IndexerBindingPropertyInfo : IBindingPropertyInfo
        {
            #region Fields

            private readonly object?[]? _indexerValues;

            private readonly bool _nonPublic;
            private readonly MemberObserver _observer;
            private readonly PropertyInfo _propertyInfo;

            private Func<object?, object?[], object?>? _getterIndexerFunc;
            private Func<object?, object?[], object?>? _setterIndexerFunc;

            #endregion

            #region Constructors

            public IndexerBindingPropertyInfo(string name, PropertyInfo propertyInfo, ParameterInfo[] indexParameters, string[] indexerValues, bool nonPublic, MemberObserver observer)
            {
                _propertyInfo = propertyInfo;
                _nonPublic = nonPublic;
                _observer = observer;
                Name = name;
                Type = _propertyInfo.PropertyType;
                _indexerValues = BindingMugenExtensions.GetIndexerValues(indexerValues!, indexParameters);

                var method = propertyInfo.GetGetMethodUnified(nonPublic);
                if (method == null)
                {
                    CanRead = false;
                    _getterIndexerFunc = MustBeReadable;
                }
                else
                {
                    CanRead = true;
                    _getterIndexerFunc = CompileIndexerGetter;
                }

                method = propertyInfo.GetSetMethodUnified(nonPublic);
                if (method == null)
                {
                    CanWrite = false;
                    _setterIndexerFunc = MustBeWritable;
                }
                else
                {
                    CanWrite = true;
                    _setterIndexerFunc = CompileIndexerSetter;
                }
            }

            #endregion

            #region Properties

            public bool IsAttached => false;

            public string Name { get; }

            public Type Type { get; }

            public object? Member => _propertyInfo;

            public BindingMemberType MemberType => BindingMemberType.Property;

            public bool CanRead { get; }

            public bool CanWrite { get; }

            #endregion

            #region Implementation of interfaces

            public IDisposable? TryObserve(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                return _observer.TryObserve(source, listener, metadata);
            }

            public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
            {
                return _getterIndexerFunc(source, _indexerValues!);
            }

            public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
            {
                var args = new object?[_indexerValues!.Length + 1];
                Array.Copy(_indexerValues, args, _indexerValues.Length);
                args[_indexerValues.Length] = value;
                _setterIndexerFunc!(source, args);
            }

            #endregion

            #region Methods

            private object? MustBeWritable(object _, object __)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
                return null;
            }

            private object MustBeReadable(object? _, object?[] __)
            {
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
                return null;
            }

            private object? CompileIndexerSetter(object? arg1, object?[] arg2)
            {
                _setterIndexerFunc = _propertyInfo.GetSetMethodUnified(_nonPublic)!.GetMethodInvoker();
                return _setterIndexerFunc(arg1, arg2);
            }

            private object? CompileIndexerGetter(object? arg, object?[] values)
            {
                _getterIndexerFunc = _propertyInfo.GetGetMethodUnified(_nonPublic)!.GetMethodInvoker();
                return _getterIndexerFunc(arg, values);
            }

            #endregion
        }

        private sealed class BindingEventInfo : IBindingEventInfo
        {
            #region Fields

            private readonly EventInfo _eventInfo;
            private readonly MemberObserver _observer;

            #endregion

            #region Constructors

            public BindingEventInfo(string name, EventInfo eventInfo, MemberObserver observer)
            {
                _eventInfo = eventInfo;
                _observer = observer;
                Name = name;
                Type = _eventInfo.EventHandlerType;
            }

            #endregion

            #region Properties

            public bool IsAttached => false;

            public string Name { get; }

            public Type Type { get; }

            public object? Member => _eventInfo;

            public BindingMemberType MemberType => BindingMemberType.Event;

            #endregion

            #region Implementation of interfaces

            public IDisposable? TrySubscribe(object? source, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                if (_observer.IsEmpty)
                    BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);

                return _observer.TryObserve(source, listener!, metadata);
            }

            #endregion
        }

        private sealed class ArrayBindingMemberInfo : IBindingPropertyInfo
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

            public bool IsAttached => false;

            public string Name { get; }

            public Type Type { get; }

            public object? Member => null;

            public BindingMemberType MemberType => BindingMemberType.Property;

            public bool CanRead => true;

            public bool CanWrite => true;

            #endregion

            #region Implementation of interfaces

            public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
            {
                Should.NotBeNull(source, nameof(source));
                return ((Array)source!).GetValue(_indexes);
            }

            public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
            {
                Should.NotBeNull(source, nameof(source));
                ((Array)source!).SetValue(value, _indexes);
            }

            public IDisposable? TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
            {
                return null;
            }

            #endregion
        }

        #endregion
    }
}