// ReSharper disable FieldCanBeMadeReadOnly.Local

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.IoC
{
    public sealed class MugenIocContainer : IocContainerBase<MugenIocContainer, MugenIocContainer>
    {
        #region Fields

        private readonly BindingDictionary _bindingRegistrations;
        private BindingDictionary? _bindingRegistrationsReadonly;
        private bool _hasConditionBinding;

        private static readonly TypeCacheDictionary TypeCacheDict = new TypeCacheDictionary();

        #endregion

        #region Constructors

        public MugenIocContainer()
            : this(null)
        {
        }

        private MugenIocContainer(MugenIocContainer? parent)
            : base(parent)
        {
            _bindingRegistrations = new BindingDictionary();
            _bindingRegistrationsReadonly = _bindingRegistrations;
            IsLockFreeRead = true;
        }

        #endregion

        #region Properties

        public static MemberFlags ConstructorMemberFlags { get; set; } = MemberFlags.InstancePublic;

        public static MemberFlags PropertyMemberFlags { get; set; } = MemberFlags.InstancePublic;

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

        protected override bool CanResolveInternal(Type service, IReadOnlyMetadataContext? metadata)
        {
            return HasRegistration(service, null, metadata) || GetTypeCache(service).IsSelfBindable;
        }

        protected override object GetInternal(Type service, IReadOnlyMetadataContext? metadata)
        {
            return GetImpl(service, null, null, metadata)!;
        }

        protected override IEnumerable<object> GetAllInternal(Type service, IReadOnlyMetadataContext? metadata)
        {
            return GetAllImpl(service, null, null, metadata)!;
        }

        protected override void BindToConstantInternal(Type service, object? instance, IReadOnlyMetadataContext? metadata)
        {
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(service, out var list))
                {
                    list = new List<BindingRegistrationBase>();
                    _bindingRegistrations[service] = list;
                }

                var binding = new ConstBindingRegistration(instance, metadata);
                list.Add(binding);
                if (binding.HasCondition)
                    _hasConditionBinding = true;
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override void BindToTypeInternal(Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata)
        {
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(service, out var list))
                {
                    list = new List<BindingRegistrationBase>();
                    _bindingRegistrations[service] = list;
                }

                BindingRegistrationBase binding;
                if (lifecycle == IocDependencyLifecycle.Singleton)
                {
                    binding = new SingletonBindingRegistration(typeTo, metadata);
                    list.Add(binding);
                }
                else if (lifecycle == IocDependencyLifecycle.Transient)
                {
                    binding = new TransientBindingRegistration(typeTo, metadata);
                    list.Add(binding);
                }
                else
                {
                    ExceptionManager.ThrowEnumOutOfRange(nameof(lifecycle), lifecycle);
                    binding = null!;
                }

                if (binding.HasCondition)
                    _hasConditionBinding = true;
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override void BindToMethodInternal(Type service, IocBindingDelegate bindingDelegate, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata)
        {
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(service, out var list))
                {
                    list = new List<BindingRegistrationBase>();
                    _bindingRegistrations[service] = list;
                }

                BindingRegistrationBase binding;
                if (lifecycle == IocDependencyLifecycle.Singleton)
                {
                    binding = new SingletonBindingRegistration(bindingDelegate, metadata);
                    list.Add(binding);
                }
                else if (lifecycle == IocDependencyLifecycle.Transient)
                {
                    binding = new TransientBindingRegistration(bindingDelegate, metadata);
                    list.Add(binding);
                }
                else
                {
                    ExceptionManager.ThrowEnumOutOfRange(nameof(lifecycle), lifecycle);
                    binding = null!;
                }
                if (binding.HasCondition)
                    _hasConditionBinding = true;
                _bindingRegistrationsReadonly = null;
            }
        }

        protected override bool UnbindInternal(Type service, IReadOnlyMetadataContext? metadata)
        {
            bool unbind;
            lock (_bindingRegistrations)
            {
                unbind = _bindingRegistrations.Remove(service);
                if (unbind)
                    _bindingRegistrationsReadonly = null;
            }

            return unbind;
        }

        private object? GetImpl(Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            var registration = GetBinding(service, parameterInfo, metadata);
            if (registration != null)
                return registration.Resolve(this, service, typeCache, parameterInfo, metadata);

            if (Parent != null && Parent.HasRegistration(service, parameterInfo, metadata))
                return Parent.GetImpl(service, typeCache, parameterInfo, metadata);

            if (TryResolve(typeCache ?? GetTypeCache(service), parameterInfo, metadata, out var value))
                return value!;

            ExceptionManager.ThrowIocCannotFindBinding(service);
            return null!;
        }

        private object?[] GetAllImpl(Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            var registrations = GetBindings(service);

            if (registrations != null && registrations.Count > 0)
            {
                if (_hasConditionBinding)
                {
                    List<object?>? result = null;
                    for (var i = 0; i < registrations.Count; i++)
                    {
                        if (registrations[i].CanResolve(this, service, parameterInfo, metadata))
                        {
                            if (result == null)
                                result = new List<object?>();
                            result.Add(registrations[i].Resolve(this, service, typeCache, parameterInfo, metadata));
                        }
                    }

                    if (result != null)
                        return result.ToArray();
                }
                else
                {
                    var result = new object?[registrations.Count];
                    for (var i = 0; i < result.Length; i++)
                        result[i] = registrations[i].Resolve(this, service, typeCache, parameterInfo, metadata);
                    return result;
                }
            }

            if (Parent != null && Parent.HasRegistration(service, parameterInfo, metadata))
                return Parent.GetAllImpl(service, typeCache, parameterInfo, metadata);

            if (TryResolve(typeCache ?? GetTypeCache(service), parameterInfo, metadata, out var value))
                return new[] { value! };
            return Default.EmptyArray<object>();
        }

        private bool TryResolve(TypeCache service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata, out object? value)
        {
            if (service.IsArray)
            {
                var elementType = service.ElementType;
                value = service.ConvertToArray(GetAllImpl(elementType.Type, elementType, parameterInfo, metadata));
                return true;
            }

            if (service.IsContainerBinding)
            {
                value = this;
                return true;
            }

            if (service.IsGenericType)
            {
                if (TryResolveOpenGeneric(service, service.GetGenericTypeDefinition().Type, parameterInfo, metadata, out value))
                    return true;
            }

            var itemType = service.TryGetCollectionItemType();
            if (itemType != null)
            {
                value = service.ConvertToCollection(GetAllImpl(itemType.Type, itemType, parameterInfo, metadata));
                return true;
            }

            var registration = service.GetSelfBindableRegistration();
            if (registration == null)
            {
                value = null;
                return false;
            }

            value = registration.Resolve(this, service.Type, service, parameterInfo, metadata);
            return true;
        }

        private bool TryResolveOpenGeneric(TypeCache service, Type genericTypeDef, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata, out object? value)
        {
            var bindingRegistration = GetBinding(genericTypeDef, parameterInfo, metadata);
            if (bindingRegistration != null)
            {
                value = bindingRegistration.Resolve(this, service.Type, service, parameterInfo, metadata);
                return true;
            }

            if (Parent != null)
                return Parent.TryResolveOpenGeneric(service, genericTypeDef, parameterInfo, metadata, out value);

            value = null;
            return false;
        }

        private bool HasRegistration(Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            var result = ContainsBinding(service, parameterInfo, metadata);
            if (result || Parent == null)
                return result;
            return Parent.HasRegistration(service, parameterInfo, metadata);
        }

        private bool ContainsBinding(Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            if (IsLockFreeRead)
                return GetReadOnlyBindingDictionary().TryGetValue(service, out var list) && HasBinding(list, service, parameterInfo, metadata);

            return ContainsBindingLockImpl(service, parameterInfo, metadata);
        }

        private BindingRegistrationBase? GetBinding(Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            if (IsLockFreeRead)
            {
                if (GetReadOnlyBindingDictionary().TryGetValue(service, out var value))
                    return FilterBinding(value, service, parameterInfo, metadata);
                return null;
            }

            return GetBindingLockImpl(service, parameterInfo, metadata);
        }

        private List<BindingRegistrationBase>? GetBindings(Type service)
        {
            if (IsLockFreeRead)
            {
                GetReadOnlyBindingDictionary().TryGetValue(service, out var value);
                return value;
            }

            return GetBindingsLockImpl(service);
        }

        private BindingRegistrationBase? GetBindingLockImpl(Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.TryGetValue(service, out var value))
                    return FilterBinding(value, service, parameterInfo, metadata);

                return null;
            }
        }

        private List<BindingRegistrationBase>? GetBindingsLockImpl(Type service)
        {
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.TryGetValue(service, out var value))
                    return value.ToList();
                return null;
            }
        }

        private bool ContainsBindingLockImpl(Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            lock (_bindingRegistrations)
            {
                return _bindingRegistrations.TryGetValue(service, out var list) && HasBinding(list, service, parameterInfo, metadata);
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

        private BindingRegistrationBase? FilterBinding(List<BindingRegistrationBase> value, Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            if (value.Count == 0)
                return null;

            if (_hasConditionBinding)
            {
                BindingRegistrationBase? result = null;
                for (var i = 0; i < value.Count; i++)
                {
                    if (value[i].CanResolve(this, service, parameterInfo, metadata))
                    {
                        if (result != null)
                            ExceptionManager.ThrowIocMoreThatOneBinding(service);
                        result = value[i];
                    }
                }

                return result;
            }

            if (value.Count > 1)
                ExceptionManager.ThrowIocMoreThatOneBinding(service);
            return value[0];
        }

        private bool HasBinding(List<BindingRegistrationBase> value, Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
        {
            if (value.Count == 0)
                return false;

            if (_hasConditionBinding)
            {
                for (var i = 0; i < value.Count; i++)
                {
                    if (value[i].CanResolve(this, service, parameterInfo, metadata))
                        return true;
                }

                return false;
            }

            return true;
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

        private sealed class ConstBindingRegistration : BindingRegistrationBase
        {
            #region Fields

            private readonly object? _instance;

            #endregion

            #region Constructors

            public ConstBindingRegistration(object? instance, IReadOnlyMetadataContext? metadata) : base(metadata)
            {
                _instance = instance;
            }

            #endregion

            #region Methods

            public override object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                return _instance;
            }

            #endregion
        }

        private sealed class SingletonBindingRegistration : FactoryBindingRegistrationBase
        {
            #region Fields

            private bool _hasValue;
            private bool _isActivating;
            private object? _value;

            #endregion

            #region Constructors

            public SingletonBindingRegistration(IocBindingDelegate bindingDelegate, IReadOnlyMetadataContext? metadata)
                : base(bindingDelegate, metadata)
            {
            }

            public SingletonBindingRegistration(Type type, IReadOnlyMetadataContext? metadata)
                : base(type, metadata)
            {
            }

            #endregion

            #region Methods

            public override object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                if (_hasValue)
                    return _value;
                return ResolveWithLock(container, service, typeCache, parameterInfo, metadata);
            }

            private object? ResolveWithLock(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                var lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_hasValue)
                        return _value;

                    if (_isActivating)
                        ExceptionManager.ThrowIocCyclicalDependency(Type?.Type ?? service);

                    _value = ResolveInternal(container, service, typeCache, parameterInfo, metadata);
                    _hasValue = true;
                    Clear();
                }
                finally
                {
                    _isActivating = false;
                    if (lockTaken)
                        Monitor.Exit(this);
                }

                container.OnActivated(service, parameterInfo?.ParameterInfo, _value, Metadata, metadata);
                return _value;
            }

            #endregion
        }

        private sealed class TransientBindingRegistration : FactoryBindingRegistrationBase
        {
            #region Fields

            private bool _isActivated;
            private bool _isActivating;

            #endregion

            #region Constructors

            public TransientBindingRegistration(IocBindingDelegate bindingDelegate, IReadOnlyMetadataContext? metadata)
                : base(bindingDelegate, metadata)
            {
            }

            public TransientBindingRegistration(Type type, IReadOnlyMetadataContext? metadata)
                : base(type, metadata)
            {
            }

            #endregion

            #region Methods

            public override object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                var result = _isActivated
                    ? ResolveInternal(container, service, typeCache, parameterInfo, metadata)
                    : ResolveWithCyclicalDependencyDetection(container, service, typeCache, parameterInfo, metadata);
                container.OnActivated(service, parameterInfo?.ParameterInfo, result, Metadata, metadata);
                return result;
            }

            private object? ResolveWithCyclicalDependencyDetection(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                var lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_isActivating)
                        ExceptionManager.ThrowIocCyclicalDependency(Type?.Type ?? service);

                    var result = ResolveInternal(container, service, typeCache, parameterInfo, metadata);
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

        private abstract class FactoryBindingRegistrationBase : BindingRegistrationBase
        {
            #region Fields

            private IocBindingDelegate? _bindingDelegate;
            protected TypeCache? Type;

            #endregion

            #region Constructors

            protected FactoryBindingRegistrationBase(IocBindingDelegate bindingDelegate, IReadOnlyMetadataContext? metadata) : base(metadata)
            {
                _bindingDelegate = bindingDelegate;
            }

            protected FactoryBindingRegistrationBase(Type type, IReadOnlyMetadataContext? metadata) : base(metadata)
            {
                Type = GetTypeCache(type);
            }

            #endregion

            #region Methods

            protected object? ResolveInternal(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                if (_bindingDelegate != null)
                    return _bindingDelegate.Invoke(container, service, parameterInfo?.ParameterInfo, Metadata, metadata);

                TypeCache type;
                if (Type!.IsGenericTypeDefinition)
                {
                    if (typeCache == null)
                        typeCache = GetTypeCache(service);

                    type = GetTypeCache(Type.Type.MakeGenericType(typeCache.GetGenericArguments()));
                }
                else
                    type = Type;

                var parameters = metadata?.Get(IocMetadata.Parameters) ?? Default.EmptyArray<IIocParameter>();
                var constructor = type.FindConstructor(parameters);
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(type.Type);

                var result = constructor!.Invoke(GetParameters(container, constructor!, parameters, metadata));
                if (parameters.Count != 0)
                    type.SetProperties(result, parameters);
                return result;
            }

            protected void Clear()
            {
                _bindingDelegate = null;
                Type = null;
            }

            private static object?[] GetParameters(MugenIocContainer container, ConstructorInfoCache constructor, IReadOnlyCollection<IIocParameter> parameters,
                IReadOnlyMetadataContext? metadata)
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
                    foreach (var injectionParameter in parameters)
                    {
                        if (!parameterInfo.CanResolveParameter(injectionParameter))
                            continue;
                        result[i] = injectionParameter.Value;
                        find = true;
                        break;
                    }

                    if (find)
                        continue;
                    var resolve = ResolveParameter(container, parameterInfo, metadata);
                    result[i] = resolve;
                }

                return result;
            }

            private static object? ResolveParameter(MugenIocContainer container, ParameterInfoCache parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                var type = parameterInfo.Type;

                //Use default parameter value.
                if (parameterInfo.IsOptional && !container.HasRegistration(type.Type, parameterInfo, metadata))
                    return parameterInfo.DefaultValue;

                //If have ParamArrayAttribute.
                if (parameterInfo.IsParams)
                {
                    var originalType = type.ElementType;

                    //If exist array binding.
                    if (container.HasRegistration(type.Type, parameterInfo, metadata))
                        return container.GetImpl(type.Type, type, parameterInfo, metadata);

                    //If exist binding for type.
                    if (container.HasRegistration(originalType.Type, parameterInfo, metadata))
                        return type.ConvertToArray(container.GetAllImpl(originalType.Type, originalType, parameterInfo, metadata));
                    return type.ConvertToArray(Default.EmptyArray<object>());
                }

                return container.GetImpl(type.Type, type, parameterInfo, metadata);
            }

            #endregion
        }

        private abstract class BindingRegistrationBase
        {
            #region Fields

            private readonly IocConditionDelegate? _condition;
            private readonly string? _name;
            private readonly int _state;
            protected readonly IReadOnlyMetadataContext? Metadata;
            private const int BothCondition = 1;
            private const int NameCondition = 2;
            private const int DelegateCondition = 3;
            private const int NoneCondition = 0;

            #endregion

            #region Constructors

            protected BindingRegistrationBase(IReadOnlyMetadataContext? metadata)
            {
                Metadata = metadata;
                if (metadata != null && metadata.Count > 0)
                {
                    _name = metadata.Get(IocMetadata.Name);
                    _condition = metadata.Get(IocMetadata.Condition);
                }

                if (_name != null && _condition != null)
                    _state = BothCondition;
                else if (_name != null)
                    _state = NameCondition;
                else if (_condition != null)
                    _state = DelegateCondition;
            }

            #endregion

            #region Properties

            public bool HasCondition => _state != NoneCondition;

            #endregion

            #region Methods

            public bool CanResolve(IIocContainer container, Type service, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata)
            {
                switch (_state)
                {
                    case BothCondition:
                        return _name!.Equals(metadata?.Get(IocMetadata.Name)) && _condition!(container, service, parameterInfo?.ParameterInfo, Metadata, metadata);
                    case NameCondition:
                        return _name!.Equals(metadata?.Get(IocMetadata.Name));
                    case DelegateCondition:
                        return _condition!(container, service, parameterInfo?.ParameterInfo, Metadata, metadata);
                }

                return true;
            }

            public abstract object? Resolve(MugenIocContainer container, Type service, TypeCache? typeCache, ParameterInfoCache? parameterInfo, IReadOnlyMetadataContext? metadata);

            #endregion
        }

        private sealed class TypeCache
        {
            #region Fields

            public readonly bool IsArray;
            public readonly bool IsContainerBinding;
            public readonly Type Type;

            private Func<object?[], object>? _arrayActivator;
            private Func<object?, object?[], object?>? _arraySetMethodInvoker;

            private List<ConstructorInfoCache>? _cachedConstructors;
            private PropertyCacheDictionary? _cachedProperties;
            private Func<object?[], object>? _collectionActivator;
            private Func<object?, object?[], object?>? _collectionAddMethodInvoker;
            private TypeCache? _collectionItemType;
            private TypeCache? _elementType;
            private Type[]? _genericArguments;
            private TypeCache? _genericTypeDefinition;

            private bool? _isCollection;
            private bool? _isGenericType;
            private bool? _isGenericTypeDefinition;
            private bool? _isSelfBindable;
            private TransientBindingRegistration? _selfBindableBindingRegistration;

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

            public TypeCache GetGenericTypeDefinition()
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
                var originalType = Type.GetGenericArgumentsUnified().First();

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
                _collectionActivator = constructor.GetActivator();
                _collectionAddMethodInvoker = methodInfo.GetMethodInvoker();
                _isCollection = true;
                return _collectionItemType;
            }

            public object ConvertToCollection(object?[] items)
            {
                var collection = _collectionActivator!(Default.EmptyArray<object>());
                var args = new object?[1];
                for (var index = 0; index < items.Length; index++)
                {
                    args[0] = items[index];
                    _collectionAddMethodInvoker!(collection, args);
                }

                return collection;
            }

            public object ConvertToArray(object?[] items)
            {
                if (_arrayActivator == null)
                {
                    var constructorInfo = Type.GetConstructorUnified(MemberFlags.InstancePublic, ArrayConstructorTypes);
                    if (constructorInfo == null)
                        ExceptionManager.ThrowCannotFindConstructor(Type);
                    _arrayActivator = constructorInfo!.GetActivator();
                }

                var array = _arrayActivator(new object[] { items.Length });
                if (_arraySetMethodInvoker == null)
                {
                    var method = typeof(MugenExtensions).GetMethodUnified(nameof(MugenExtensions.InitializeArray), MemberFlags.StaticOnly);
                    Should.BeSupported(method != null, typeof(MugenExtensions).Name + "." + nameof(MugenExtensions.InitializeArray));
                    _arraySetMethodInvoker = method!.MakeGenericMethod(ElementType.Type).GetMethodInvoker();
                }

                _arraySetMethodInvoker.Invoke(null, new[] { array, items });
                return array;
            }

            public TransientBindingRegistration? GetSelfBindableRegistration()
            {
                if (!IsSelfBindable)
                    return null;

                if (_selfBindableBindingRegistration == null)
                    _selfBindableBindingRegistration = new TransientBindingRegistration(Type, null);
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
            private ParameterInfoCache[]? _parameters;

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
                    _activatorDel = _constructor.GetActivator();
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

            public readonly bool IsOptional;

            public readonly ParameterInfo ParameterInfo;
            public readonly TypeCache Type;
            private object? _defaultValue;
            private bool _hasDefValue;
            private bool? _isParams;

            #endregion

            #region Constructors

            public ParameterInfoCache(ParameterInfo parameterInfo)
            {
                ParameterInfo = parameterInfo;
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
                        _isParams = ParameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
                    return _isParams.Value;
                }
            }

            public object? DefaultValue
            {
                get
                {
                    if (!_hasDefValue)
                    {
                        _defaultValue = ParameterInfo.DefaultValue;
                        _hasDefValue = true;
                    }

                    return _defaultValue;
                }
            }

            #endregion

            #region Methods

            public bool CanResolveParameter(IIocParameter parameter)
            {
                return parameter.ParameterType == IocParameterType.Constructor && parameter.Name == ParameterInfo.Name;
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

        private sealed class BindingDictionary : LightDictionaryBase<Type, List<BindingRegistrationBase>>
        {
            #region Constructors

            public BindingDictionary()
                : base(7)
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
                Clone(bindingDictionary, list => list?.ToList()!);
                return bindingDictionary;
            }

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

        #endregion
    }
}