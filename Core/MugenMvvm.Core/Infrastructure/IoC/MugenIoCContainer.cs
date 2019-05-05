// ReSharper disable FieldCanBeMadeReadOnly.Local

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.IoC
{
    public sealed class MugenIocContainer : IocContainerBase<MugenIocContainer, MugenIocContainer>
    {
        #region Fields

        private readonly BindingDictionary _bindingRegistrations;
        private BindingDictionary? _bindingRegistrationsReadonly;

        private static readonly TypeCacheDictionary TypeCacheDict;

        #endregion

        #region Constructors

        static MugenIocContainer()
        {
            TypeCacheDict = new TypeCacheDictionary();
            ConstructorMemberFlags = MemberFlags.InstancePublic;
            PropertyMemberFlags = MemberFlags.InstancePublic;
        }

        public MugenIocContainer()
            : this(null)
        {
        }

        private MugenIocContainer(MugenIocContainer parent)
            : base(parent)
        {
            _bindingRegistrations = new BindingDictionary();
            _bindingRegistrationsReadonly = _bindingRegistrations;
            IsLockFreeRead = true;
        }

        #endregion

        #region Properties

        public static MemberFlags ConstructorMemberFlags { get; set; }

        public static MemberFlags PropertyMemberFlags { get; set; }

        public bool IsLockFreeRead { get; set; }

        public override MugenIocContainer Container => this;

        #endregion

        #region Methods

        protected override void OnDispose()
        {
            lock (_bindingRegistrations)
            {
                _bindingRegistrations.Clear();
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override MugenIocContainer CreateChildInternal(IReadOnlyMetadataContext? metadata)
        {
            return new MugenIocContainer(this)
            {
                IsLockFreeRead = IsLockFreeRead
            };
        }

        protected override bool CanResolveInternal(Type service, string? name, IReadOnlyMetadataContext? metadata)
        {
            return CanResolveImpl(service, null, name);
        }

        protected override object GetInternal(Type service, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata)
        {
            return GetImpl(service, null, name, parameters, metadata);
        }

        protected override IEnumerable<object> GetAllInternal(Type service, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata)
        {
            return GetAllImpl(service, null, name, parameters, metadata);
        }

        protected override void BindToConstantInternal(Type service, object? instance, string? name, IReadOnlyMetadataContext? metadata)
        {
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<IBindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                list.Add(new ConstBindingRegistration(instance));
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override void BindToTypeInternal(Type service, Type typeTo, IocDependencyLifecycle lifecycle, string? name, IReadOnlyCollection<IIocParameter> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<IBindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                if (lifecycle == IocDependencyLifecycle.Singleton)
                    list.Add(new SingletonBindingRegistration(typeTo, parameters, metadata));
                else if (lifecycle == IocDependencyLifecycle.Transient)
                    list.Add(new TransientBindingRegistration(typeTo, parameters, metadata));
                else
                    ExceptionManager.ThrowEnumOutOfRange(nameof(lifecycle), lifecycle);
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override void BindToMethodInternal(Type service, Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IocDependencyLifecycle lifecycle, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata)
        {
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<IBindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                if (lifecycle == IocDependencyLifecycle.Singleton)
                    list.Add(new SingletonBindingRegistration(methodBindingDelegate, parameters, metadata));
                else if (lifecycle == IocDependencyLifecycle.Transient)
                    list.Add(new TransientBindingRegistration(methodBindingDelegate, parameters, metadata));
                else
                    ExceptionManager.ThrowEnumOutOfRange(nameof(lifecycle), lifecycle);
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override bool UnbindInternal(Type service, string? name, IReadOnlyMetadataContext? metadata)
        {
            bool unbind;
            lock (_bindingRegistrations)
            {
                if (name == null)
                {
                    var keys = _bindingRegistrations.Where(pair => pair.Key.Type == service).ToArray();
                    for (var index = 0; index < keys.Length; index++)
                        _bindingRegistrations.Remove(keys[index].Key);
                    unbind = keys.Length != 0;
                }
                else
                    unbind = _bindingRegistrations.Remove(new BindingKey(service, name));

                if (unbind)
                    _bindingRegistrationsReadonly = null;
            }

            return unbind;
        }

        private bool CanResolveImpl(Type service, TypeCache? typeCache, string? name)
        {
            var key = new BindingKey(service, name);
            return CanResolveImpl(service, typeCache, ref key);
        }

        private bool CanResolveImpl(Type service, TypeCache? typeCache, ref BindingKey key)
        {
            if (ContainsBinding(ref key))
                return true;

            if (Parent == null)
                return (typeCache ?? GetTypeCache(service)).IsSelfBindable;
            return Parent.CanResolveImpl(service, typeCache, ref key);
        }

        private object? GetImpl(Type service, TypeCache? typeCache, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata)
        {
            var key = new BindingKey(service, name);

            var registration = GetBinding(ref key);
            if (registration != null)
                return registration.Resolve(this, service, typeCache, parameters, metadata);

            if (Parent != null && Parent.HasRegistration(ref key))
                return Parent.GetImpl(service, typeCache, name, parameters, metadata);

            if (TryResolve(typeCache ?? GetTypeCache(service), ref key, parameters, metadata, out var value))
                return value!;

            ExceptionManager.ThrowIocCannotFindBinding(service);
            return null!;
        }

        private object[] GetAllImpl(Type service, TypeCache? typeCache, string? name, IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata)
        {
            var key = new BindingKey(service, name);
            var registrations = GetBindings(ref key);

            if (registrations != null && registrations.Count > 0)
            {
                var result = new object[registrations.Count];
                for (var i = 0; i < result.Length; i++)
                    result[i] = registrations[i].Resolve(this, service, typeCache, parameters, metadata);
                return result;
            }

            if (Parent != null && Parent.HasRegistration(ref key))
                return Parent.GetAllImpl(service, typeCache, name, parameters, metadata);

            if (TryResolve(typeCache ?? GetTypeCache(service), ref key, parameters, metadata, out var value))
                return new[] { value! };
            return Default.EmptyArray<object>();
        }

        private bool TryResolve(TypeCache service, ref BindingKey key, IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata, out object? value)
        {
            if (service.IsArray)
            {
                var elementType = service.ElementType;
                value = service.ConvertToArray(GetAllImpl(elementType.Type, elementType, null, parameters, metadata));
                return true;
            }

            if (service.IsContainerBinding)
            {
                value = this;
                return true;
            }

            if (service.IsGenericType)
            {
                var typeDefKey = new BindingKey(service.GetGenericTypeDefinition().Type, key.Name);
                if (TryResolveOpenGeneric(service, ref typeDefKey, parameters, metadata, out value))
                    return true;
            }

            var itemType = service.TryGetCollectionItemType();
            if (itemType != null)
            {
                value = service.ConvertToCollection(GetAllImpl(itemType.Type, itemType, null, parameters, metadata));
                return true;
            }

            var registration = service.GetSelfBindableRegistration();
            if (registration == null)
            {
                value = null;
                return false;
            }

            value = registration.Resolve(this, service.Type, service, parameters, metadata);
            return true;
        }

        private bool TryResolveOpenGeneric(TypeCache service, ref BindingKey typeDefKey, IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata,
            out object? value)
        {
            var bindingRegistration = GetBinding(ref typeDefKey);
            if (bindingRegistration != null)
            {
                value = bindingRegistration.Resolve(this, service.Type, service, parameters, metadata);
                return true;
            }

            if (Parent != null)
                return Parent.TryResolveOpenGeneric(service, ref typeDefKey, parameters, metadata, out value);

            value = null;
            return false;
        }

        private bool HasRegistration(ref BindingKey key)
        {
            var result = ContainsBinding(ref key);
            if (result || Parent == null)
                return result;
            return Parent.HasRegistration(ref key);
        }

        private bool ContainsBinding(ref BindingKey key)
        {
            if (IsLockFreeRead)
                return GetReadOnlyBindingDictionary().ContainsKey(key);

            return ContainsBindingLockImpl(ref key);
        }

        private IBindingRegistration? GetBinding(ref BindingKey key)
        {
            if (IsLockFreeRead)
            {
                if (!GetReadOnlyBindingDictionary().TryGetValue(key, out var value))
                    return null;

                if (value.Count == 0)
                    return null;

                if (value.Count > 1)
                    ExceptionManager.ThrowIocMoreThatOneBinding(key.Type);
                return value[0];
            }

            return GetBindingLockImpl(ref key);
        }

        private List<IBindingRegistration>? GetBindings(ref BindingKey key)
        {
            if (IsLockFreeRead)
            {
                GetReadOnlyBindingDictionary().TryGetValue(key, out var value);
                return value;
            }

            return GetBindingsLockImpl(ref key);
        }

        private IBindingRegistration? GetBindingLockImpl(ref BindingKey key)
        {
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.TryGetValue(key, out var value))
                    return null;

                if (value.Count == 0)
                    return null;

                if (value.Count > 1)
                    ExceptionManager.ThrowIocMoreThatOneBinding(key.Type);
                return value[0];
            }
        }

        private List<IBindingRegistration>? GetBindingsLockImpl(ref BindingKey key)
        {
            lock (_bindingRegistrations)
            {
                _bindingRegistrations.TryGetValue(key, out var value);
                return value.ToList();
            }
        }

        private bool ContainsBindingLockImpl(ref BindingKey key)
        {
            lock (_bindingRegistrations)
            {
                return _bindingRegistrations.ContainsKey(key);
            }
        }

        private BindingDictionary GetReadOnlyBindingDictionary()
        {
            var dictionary = _bindingRegistrationsReadonly;
            if (dictionary == null)
            {
                lock (_bindingRegistrations)
                {
                    dictionary = _bindingRegistrations.Clone();
                    _bindingRegistrationsReadonly = dictionary;
                }
            }

            return dictionary;
        }

        private static TypeCache GetTypeCache(Type type)
        {
            lock (TypeCacheDict)
            {
                if (!TypeCacheDict.TryGetValue(type, out var value))
                {
                    value = new TypeCache(type);
                    TypeCacheDict[type] = value;
                }

                return value;
            }
        }

        #endregion

        #region Nested types

        private interface IBindingRegistration
        {
            object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata);
        }

        private sealed class ConstBindingRegistration : IBindingRegistration
        {
            #region Fields

            private readonly object _instance;

            #endregion

            #region Constructors

            public ConstBindingRegistration(object instance)
            {
                _instance = instance;
            }

            #endregion

            #region Implementation of interfaces

            public object Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext metadata)
            {
                return _instance;
            }

            #endregion
        }

        private sealed class SingletonBindingRegistration : BindingRegistrationBase
        {
            #region Fields

            private bool _hasValue;
            private bool _isActivating;
            private object? _value;

            #endregion

            #region Constructors

            public SingletonBindingRegistration(Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
                IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata)
                : base(methodBindingDelegate, parameters, metadata)
            {
            }

            public SingletonBindingRegistration(Type type, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata)
                : base(type, parameters, metadata)
            {
            }

            #endregion

            #region Methods

            public override object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter>? parameters,
                IReadOnlyMetadataContext? metadata)
            {
                if (_hasValue)
                    return _value;
                return ResolveWithLock(container, service, typeCache, parameters, metadata);
            }

            private object? ResolveWithLock(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext metadata)
            {
                var lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_hasValue)
                        return _value;

                    if (_isActivating)
                        ExceptionManager.ThrowIocCyclicalDependency(Type?.Type ?? service);

                    _value = ResolveInternal(container, service, typeCache, parameters, ref metadata);
                    _hasValue = true;
                    Clear();
                }
                finally
                {
                    _isActivating = false;
                    if (lockTaken)
                        Monitor.Exit(this);
                }

                container.OnActivated(service, _value, metadata);
                return _value;
            }

            #endregion
        }

        private sealed class TransientBindingRegistration : BindingRegistrationBase
        {
            #region Fields

            private bool _isActivated;
            private bool _isActivating;

            #endregion

            #region Constructors

            public TransientBindingRegistration(Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
                IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext metadata)
                : base(methodBindingDelegate, parameters, metadata)
            {
            }

            public TransientBindingRegistration(Type type, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext metadata)
                : base(type, parameters, metadata)
            {
            }

            #endregion

            #region Methods

            public override object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter> parameters,
                IReadOnlyMetadataContext metadata)
            {
                var result = _isActivated
                    ? ResolveInternal(container, service, typeCache, parameters, ref metadata)
                    : ResolveWithCyclicalDependencyDetection(container, service, typeCache, parameters, ref metadata);
                container.OnActivated(service, result, metadata);
                return result;
            }

            private object? ResolveWithCyclicalDependencyDetection(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter> parameters,
                ref IReadOnlyMetadataContext metadata)
            {
                var lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_isActivating)
                        ExceptionManager.ThrowIocCyclicalDependency(Type?.Type ?? service);

                    var result = ResolveInternal(container, service, typeCache, parameters, ref metadata);
                    _isActivated = true;
                    return result;
                }
                finally
                {
                    _isActivating = false;
                    if (lockTaken)
                        Monitor.Exit(this);
                }
            }

            #endregion
        }

        private abstract class BindingRegistrationBase : IBindingRegistration
        {
            #region Fields

            private IReadOnlyMetadataContext? _metadata;
            private Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object>? _methodBindingDelegate;
            private IReadOnlyCollection<IIocParameter>? _parameters;
            protected TypeCache? Type;

            #endregion

            #region Constructors

            protected BindingRegistrationBase(Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
                IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata)
            {
                _methodBindingDelegate = methodBindingDelegate;
                _parameters = parameters;
                _metadata = metadata;
            }

            protected BindingRegistrationBase(Type type, IReadOnlyCollection<IIocParameter>? parameters, IReadOnlyMetadataContext? metadata)
            {
                Type = GetTypeCache(type);
                _parameters = parameters;
                _metadata = metadata;
            }

            #endregion

            #region Implementation of interfaces

            public abstract object Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter>? parameters,
                IReadOnlyMetadataContext? metadata);

            #endregion

            #region Methods

            protected object? ResolveInternal(MugenIocContainer container, Type service, TypeCache? typeCache, IReadOnlyCollection<IIocParameter> parameters,
                ref IReadOnlyMetadataContext metadata)
            {
                var originalParameters = parameters;
                var originalMetadata = metadata;
                parameters = MergeParameters(parameters);
                metadata = MergeMetadata(metadata);
                if (_methodBindingDelegate != null)
                    return _methodBindingDelegate.Invoke(container, parameters, metadata);

                TypeCache type;
                if (Type.IsGenericTypeDefinition)
                {
                    if (typeCache == null)
                        typeCache = GetTypeCache(service);

                    type = GetTypeCache(Type.Type.MakeGenericType(typeCache.GetGenericArguments()));
                }
                else
                    type = Type;

                var constructor = type.FindConstructor(parameters);
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(type.Type);

                var result = constructor!.Invoke(GetParameters(container, constructor!, originalParameters, parameters, originalMetadata));
                if (parameters.Count != 0)
                    type.SetProperties(result, parameters);
                return result;
            }

            protected void Clear()
            {
                _metadata = null;
                _methodBindingDelegate = null;
                Type = null;
                _parameters = null;
            }

            private IReadOnlyCollection<IIocParameter> MergeParameters(IReadOnlyCollection<IIocParameter>? parameters)
            {
                if (_parameters == null || _parameters.Count == 0)
                    return parameters ?? Default.EmptyArray<IIocParameter>();
                if (parameters == null || parameters.Count == 0)
                    return _parameters ?? Default.EmptyArray<IIocParameter>();
                var iocParameters = new List<IIocParameter>(parameters);
                iocParameters.AddRange(_parameters);
                return iocParameters;
            }

            private IReadOnlyMetadataContext MergeMetadata(IReadOnlyMetadataContext? metadata)
            {
                if (_metadata == null || _metadata.Count == 0)
                    return metadata.DefaultIfNull();

                if (metadata == null || metadata.Count == 0)
                    return _metadata.DefaultIfNull();

                var metadataContext = MugenExtensions.GetMetadataContext(null, metadata);
                metadataContext.Merge(_metadata);
                return metadataContext;
            }

            private static object?[] GetParameters(MugenIocContainer container, ConstructorInfoCache constructor, IReadOnlyCollection<IIocParameter>? parameters,
                IReadOnlyCollection<IIocParameter> mergedParameters, IReadOnlyMetadataContext metadata)
            {
                var parameterInfos = constructor.GetParameters();
                if (parameterInfos.Length == 0)
                    return Default.EmptyArray<object?>();
                var result = new object?[parameterInfos.Length];
                //Find constructor arguments
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    var find = false;
                    foreach (var injectionParameter in mergedParameters)
                    {
                        if (!parameterInfo.CanResolveParameter(injectionParameter))
                            continue;
                        result[i] = injectionParameter.Value;
                        find = true;
                        break;
                    }

                    if (find)
                        continue;
                    var resolve = ResolveParameter(container, parameterInfo, parameters, metadata);
                    result[i] = resolve;
                }

                return result;
            }

            private static object? ResolveParameter(MugenIocContainer container, ParameterInfoCache parameterInfo, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext metadata)
            {
                var type = parameterInfo.Type;

                //Use default parameter value.
                if (parameterInfo.IsOptional && !container.CanResolveImpl(type.Type, type, null))
                    return parameterInfo.DefaultValue;

                //If have ParamArrayAttribute.
                if (parameterInfo.IsParams)
                {
                    var originalType = type.ElementType;

                    //If exist array binding.
                    if (container.CanResolveImpl(type.Type, type, null))
                        return container.GetImpl(type.Type, type, null, parameters, metadata);

                    //If exist binding for type.
                    if (container.CanResolveImpl(originalType.Type, originalType, null))
                        return type.ConvertToArray(container.GetAllImpl(originalType.Type, originalType, null, null, metadata));
                    return type.ConvertToArray(Default.EmptyArray<object>());
                }

                return container.GetImpl(type.Type, type, null, parameters, metadata);
            }

            #endregion
        }

        private sealed class TypeCache
        {
            #region Fields

            public readonly bool IsArray;
            public readonly bool IsContainerBinding;
            public readonly Type Type;

            private Func<object?[], object>? _arrayActivatorDelegate;
            private Func<object?, object?[], object>? _arraySetMethodDelegate;

            private List<ConstructorInfoCache>? _cachedConstructors;
            private PropertyCacheDictionary _cachedProperties;
            private Func<object?[], object>? _collectionActivatorDelegate;
            private Func<object?, object?[], object>? _collectionAddMethodDelegate;
            private TypeCache? _collectionItemType;
            private TypeCache? _elementType;
            private Type[]? _genericArguments;
            private TypeCache? _genericTypeDefinition;

            private bool? _isCollection;
            private bool? _isGenericType;
            private bool? _isGenericTypeDefinition;
            private bool? _isSelfBindable;
            private TransientBindingRegistration _selfBindableBindingRegistration;

            private static readonly Type[] ArrayConstructorTypes = { typeof(int) };

            #endregion

            #region Constructors

            public TypeCache(Type type)
            {
                Type = type;
                IsArray = type.IsArray;
                IsContainerBinding = type.EqualsEx(typeof(IServiceProvider)) || type.EqualsEx(typeof(IIocContainer));
            }

            #endregion

            #region Properties

            public bool IsGenericType
            {
                get
                {
                    if (_isGenericType == null)
                        _isGenericType = Type.IsGenericTypeUnified();
                    return _isGenericType.Value;
                }
            }

            public bool IsGenericTypeDefinition
            {
                get
                {
                    if (_isGenericTypeDefinition == null)
                        _isGenericTypeDefinition = Type.IsGenericTypeDefinitionUnified();
                    return _isGenericTypeDefinition.Value;
                }
            }

            public bool IsSelfBindable
            {
                get
                {
                    if (_isSelfBindable == null)
                    {
                        _isSelfBindable = !Type.IsInterfaceUnified()
                                          && !Type.IsValueTypeUnified()
                                          && Type != typeof(string)
                                          && !Type.IsAbstractUnified()
                                          && !Type.ContainsGenericParametersUnified();
                    }

                    return _isSelfBindable.Value;
                }
            }

            public TypeCache ElementType
            {
                get
                {
                    if (_elementType == null)
                        _elementType = GetTypeCache(Type.GetElementType());
                    return _elementType;
                }
            }

            #endregion

            #region Methods

            public TypeCache? GetGenericTypeDefinition()
            {
                if (_genericTypeDefinition == null)
                    _genericTypeDefinition = GetTypeCache(Type.GetGenericTypeDefinition());
                return _genericTypeDefinition;
            }

            public TypeCache? TryGetCollectionItemType()
            {
                if (_isCollection != null)
                    return _collectionItemType;

                if (!IsGenericType)
                {
                    _isCollection = false;
                    return null;
                }

                var definition = Type.GetGenericTypeDefinition();
                var originalType = Type.GetGenericArgumentsUnified()[0];

                ConstructorInfo? constructor = null;
                if (definition.IsInterfaceUnified())
                {
                    if (definition == typeof(ICollection<>) || definition == typeof(IEnumerable<>) || definition == typeof(IList<>)
                        || definition == typeof(IReadOnlyCollection<>) || definition == typeof(IReadOnlyList<>))
                        constructor = typeof(List<>).MakeGenericType(originalType).GetConstructorUnified(MemberFlags.InstancePublic, Default.EmptyArray<Type>());
                }
                else
                {
                    if (typeof(ICollection<>).MakeGenericType(originalType).IsAssignableFromUnified(Type))
                        constructor = Type.GetConstructorUnified(MemberFlags.InstancePublic, Default.EmptyArray<Type>());
                }

                if (constructor == null)
                {
                    _isCollection = false;
                    return null;
                }

                var methodInfo = constructor.DeclaringType?.GetMethodUnified("Add", MemberFlags.InstancePublic, originalType);
                if (methodInfo == null || methodInfo.IsStatic)
                {
                    _isCollection = false;
                    return null;
                }

                _collectionItemType = GetTypeCache(originalType);
                _collectionActivatorDelegate = constructor.GetActivatorDelegate();
                _collectionAddMethodDelegate = methodInfo.GetMethodDelegate();
                _isCollection = true;
                return _collectionItemType;
            }

            public object ConvertToCollection(object[] items)
            {
                var collection = _collectionActivatorDelegate(Default.EmptyArray<object>());
                var args = new object[1];
                for (var index = 0; index < items.Length; index++)
                {
                    args[0] = items[index];
                    _collectionAddMethodDelegate(collection, args);
                }

                return collection;
            }

            public object ConvertToArray(object[] items)
            {
                if (_arrayActivatorDelegate == null)
                {
                    var constructorInfo = Type.GetConstructorUnified(MemberFlags.InstancePublic, ArrayConstructorTypes);
                    if (constructorInfo == null)
                        ExceptionManager.ThrowCannotFindConstructor(Type);
                    _arrayActivatorDelegate = constructorInfo.GetActivatorDelegate();
                }

                var array = _arrayActivatorDelegate(new object[] { items.Length });
                if (_arraySetMethodDelegate == null)
                {
                    var method = typeof(MugenExtensions).GetMethodUnified(nameof(MugenExtensions.InitializeArray), MemberFlags.StaticOnly);
                    Should.BeSupported(method != null, typeof(MugenExtensions).Name + "." + nameof(MugenExtensions.InitializeArray));
                    _arraySetMethodDelegate = method.MakeGenericMethod(ElementType.Type).GetMethodDelegate();
                }

                _arraySetMethodDelegate.Invoke(null, new[] { array, items });
                return array;
            }

            public TransientBindingRegistration GetSelfBindableRegistration()
            {
                if (!IsSelfBindable)
                    return null;

                if (_selfBindableBindingRegistration == null)
                    _selfBindableBindingRegistration = new TransientBindingRegistration(Type, null, null);
                return _selfBindableBindingRegistration;
            }

            public void SetProperties(object item, IReadOnlyCollection<IIocParameter> parameters)
            {
                PropertyCacheDictionary? props = null;
                foreach (var iocParameter in parameters)
                {
                    if (iocParameter.ParameterType != IocParameterType.Property)
                        continue;
                    if (props == null)
                        props = GetCachedProperties();

                    if (props.TryGetValue(iocParameter.Name, out var value))
                        value.Invoke(item, iocParameter.Value);
                }
            }

            public ConstructorInfoCache? FindConstructor(IReadOnlyCollection<IIocParameter> parameters)
            {
                ConstructorInfoCache? result = null;
                var bestCount = -1;
                var currentCountParameter = 0;
                var constructorInfos = GetConstructors();
                if (constructorInfos.Count == 1)
                    return constructorInfos[0];

                for (var index = 0; index < constructorInfos.Count; index++)
                {
                    var constructorInfo = constructorInfos[index];
                    var currentCount = 0;
                    var parameterInfos = constructorInfo.GetParameters();
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        foreach (var parameter in parameters)
                        {
                            if (parameterInfo.CanResolveParameter(parameter))
                            {
                                currentCount++;
                                break;
                            }
                        }
                    }

                    if (bestCount > currentCount)
                        continue;
                    if (bestCount == currentCount && parameterInfos.Length > currentCountParameter)
                        continue;
                    currentCountParameter = parameterInfos.Length;
                    result = constructorInfo;
                    bestCount = currentCount;
                }

                return result;
            }

            private PropertyCacheDictionary GetCachedProperties()
            {
                if (_cachedProperties == null)
                {
                    var cachedProperties = new PropertyCacheDictionary();
                    foreach (var propertyInfo in Type.GetPropertiesUnified(PropertyMemberFlags))
                    {
                        var method = propertyInfo.GetSetMethodUnified(PropertyMemberFlags.HasMemberFlag(MemberFlags.NonPublic));
                        if (method != null)
                            cachedProperties[propertyInfo.Name] = new PropertyInfoCache(propertyInfo);
                    }

                    _cachedProperties = cachedProperties;
                }

                return _cachedProperties;
            }

            private List<ConstructorInfoCache> GetConstructors()
            {
                if (_cachedConstructors == null)
                {
                    var cachedConstructors = new List<ConstructorInfoCache>();
                    foreach (var constructorInfo in Type.GetConstructorsUnified(ConstructorMemberFlags))
                    {
                        if (ConstructorInfoCache.IsValid(constructorInfo, ConstructorMemberFlags))
                            cachedConstructors.Add(new ConstructorInfoCache(constructorInfo));
                    }

                    _cachedConstructors = cachedConstructors;
                }

                return _cachedConstructors;
            }

            public Type[] GetGenericArguments()
            {
                if (_genericArguments == null)
                {
                    var list = Type.GetGenericArgumentsUnified();
                    _genericArguments = list as Type[] ?? list.ToArray();
                }

                return _genericArguments;
            }

            #endregion
        }

        private sealed class ConstructorInfoCache
        {
            #region Fields

            private readonly ConstructorInfo _constructor;
            private Func<object?[], object>? _activatorDel;
            private ParameterInfoCache[] _parameters;

            #endregion

            #region Constructors

            public ConstructorInfoCache(ConstructorInfo constructor)
            {
                _constructor = constructor;
            }

            #endregion

            #region Methods

            public static bool IsValid(ConstructorInfo constructor, MemberFlags memberFlags)
            {
                if (constructor.IsStatic)
                    return false;
                return constructor.IsPublic || memberFlags.HasMemberFlag(MemberFlags.NonPublic);
            }

            public ParameterInfoCache[] GetParameters()
            {
                if (_parameters == null)
                {
                    var parameterInfos = _constructor.GetParameters();
                    var parameters = new ParameterInfoCache[parameterInfos.Length];
                    for (var i = 0; i < parameters.Length; i++)
                        parameters[i] = new ParameterInfoCache(parameterInfos[i]);
                    _parameters = parameters;
                }

                return _parameters;
            }

            public object Invoke(params object?[] parameters)
            {
                if (_activatorDel == null)
                    _activatorDel = _constructor.GetActivatorDelegate();
                return _activatorDel(parameters);
            }

            #endregion
        }

        private sealed class PropertyInfoCache
        {
            #region Fields

            private readonly PropertyInfo _propertyInfo;
            private Action<object?, object?>? _invokeAction;

            #endregion

            #region Constructors

            public PropertyInfoCache(PropertyInfo propertyInfo)
            {
                _propertyInfo = propertyInfo;
            }

            #endregion

            #region Methods

            public void Invoke(object item, object value)
            {
                if (_invokeAction == null)
                    _invokeAction = _propertyInfo.GetMemberSetter<object?>();
                _invokeAction(item, value);
            }

            #endregion
        }

        private sealed class ParameterInfoCache
        {
            #region Fields

            private readonly ParameterInfo _parameterInfo;
            public readonly bool IsOptional;
            public readonly TypeCache Type;
            private object? _defaultValue;
            private bool _hasDefValue;
            private bool? _isParams;

            #endregion

            #region Constructors

            public ParameterInfoCache(ParameterInfo parameterInfo)
            {
                _parameterInfo = parameterInfo;
                Type = GetTypeCache(parameterInfo.ParameterType);
                IsOptional = parameterInfo.IsOptional;
            }

            #endregion

            #region Properties

            public bool IsParams
            {
                get
                {
                    if (_isParams == null)
                        _isParams = _parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
                    return _isParams.Value;
                }
            }

            public object? DefaultValue
            {
                get
                {
                    if (!_hasDefValue)
                    {
                        _defaultValue = _parameterInfo.DefaultValue;
                        _hasDefValue = true;
                    }

                    return _defaultValue;
                }
            }

            #endregion

            #region Methods

            public bool CanResolveParameter(IIocParameter parameter)
            {
                return parameter.ParameterType == IocParameterType.Constructor && parameter.Name == _parameterInfo.Name;
            }

            #endregion
        }

        private sealed class TypeCacheDictionary : LightDictionaryBase<Type, TypeCache>
        {
            #region Constructors

            public TypeCacheDictionary() : base(17)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(Type x, Type y)
            {
                return x.EqualsEx(y);
            }

            protected override int GetHashCode(Type key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class PropertyCacheDictionary : LightDictionaryBase<string, PropertyInfoCache>
        {
            #region Constructors

            public PropertyCacheDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class BindingDictionary : LightDictionaryBase<BindingKey, List<IBindingRegistration>>
        {
            #region Constructors

            public BindingDictionary()
                : base(3)
            {
            }

            private BindingDictionary(bool initialize) : base(initialize)
            {
            }

            #endregion

            #region Methods

            public BindingDictionary Clone()
            {
                var bindingDictionary = new BindingDictionary(false);
                Clone(bindingDictionary, list => list?.ToList());
                return bindingDictionary;
            }

            protected override bool Equals(BindingKey x, BindingKey y)
            {
                return x.Type.EqualsEx(y.Type) && string.Equals(x.Name, y.Name);
            }

            protected override int GetHashCode(BindingKey key)
            {
                unchecked
                {
                    return key.Type.GetHashCode() * 397 ^ (key.Name?.GetHashCode() ?? 0);
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct BindingKey
        {
            #region Fields

            public string? Name;
            public Type Type;

            #endregion

            #region Constructors

            public BindingKey(Type type, string? name)
            {
                Type = type;
                Name = name;
            }

            #endregion
        }

        #endregion
    }
}