using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure.IoC
{
    public class MugenIoCContainer : IIoCContainer
    {
        #region Fields

        private readonly Dictionary<BindingKey, List<BindingRegistration>> _bindingRegistrations;
        private readonly MugenIoCContainer? _parent;
        private readonly Dictionary<Type, BindingRegistration> _selfActivatedRegistrations;

        private bool _isDisposed;

        private static int _idCounter;

        #endregion

        #region Constructors

        public MugenIoCContainer()
        {
            _bindingRegistrations = new Dictionary<BindingKey, List<BindingRegistration>>(BindingKey.TypeNameComparer);
            _selfActivatedRegistrations = new Dictionary<Type, BindingRegistration>();
            Id = Interlocked.Increment(ref _idCounter);
            ConstructorMemberFlags = MemberFlags.InstancePublic;
            PropertyMemberFlags = MemberFlags.InstancePublic;
        }

        private MugenIoCContainer(MugenIoCContainer parent)
            : this()
        {
            _parent = parent;
        }

        #endregion

        #region Properties

        public MemberFlags ConstructorMemberFlags { get; set; }

        public MemberFlags PropertyMemberFlags { get; set; }

        public int Id { get; }

        public IIoCContainer? Parent => _parent;

        public object Container => this;

        #endregion

        #region Implementation of interfaces

        public IIoCContainer CreateChild()
        {
            NotBeDisposed();
            return new MugenIoCContainer(this);
        }

        public bool CanResolve(Type service, string? name = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            return CanResolve(service, ref key);
        }

        public object Get(Type service, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            BindingRegistration? registration = null;
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.TryGetValue(key, out var list))
                {
                    if (list.Count > 1)
                        ExceptionManager.ThrowIoCMoreThatOneBinding(service);
                    if (list.Count == 1)
                        registration = list[0];
                }
            }

            if (registration != null)
                return registration.Resolve(parameters);
            if (_parent != null && _parent.HasRegistration(ref key))
                return _parent.Get(service, name, parameters);
            return Resolve(service, parameters);
        }

        IEnumerable<object> IIoCContainer.GetAll(Type service, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            return GetAll(service, name, parameters);
        }

        public void BindToType(Type service, Type typeTo, IoCDependencyLifecycle lifecycle, string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                list.Add(new BindingRegistration(this, typeTo, lifecycle, parameters));
            }
        }

        public void BindToConstant(Type service, object instance, string? name = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                list.Add(new BindingRegistration(this, instance));
            }
        }

        public void BindToMethod(Type service, Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, object> methodBindingDelegate, IoCDependencyLifecycle lifecycle,
            string? name = null, IReadOnlyCollection<IIoCParameter>? parameters = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            var key = new BindingKey(service, name);
            lock (_bindingRegistrations)
            {
                if (!_bindingRegistrations.TryGetValue(key, out var list))
                {
                    list = new List<BindingRegistration>();
                    _bindingRegistrations[key] = list;
                }

                list.Add(new BindingRegistration(this, methodBindingDelegate, lifecycle, parameters));
            }
        }

        public void Unbind(Type service)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            lock (_bindingRegistrations)
            {
                var keys = _bindingRegistrations.Keys.Where(key => key.Type == service).ToArray();
                for (var index = 0; index < keys.Length; index++)
                    _bindingRegistrations.Remove(keys[index]);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            lock (_bindingRegistrations)
            {
                _bindingRegistrations.Clear();
            }

            lock (_selfActivatedRegistrations)
            {
                _selfActivatedRegistrations.Clear();
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        bool IServiceProviderEx.TryGetService(Type serviceType, out object? service)
        {
            try
            {
                if (CanResolve(serviceType))
                {
                    service = Get(serviceType);
                    return true;
                }
            }
            catch
            {
                ;
            }

            service = null;
            return false;
        }

        #endregion

        #region Methods

        private object[] GetAll(Type service, string? name, IReadOnlyCollection<IIoCParameter>? parameters)
        {
            var key = new BindingKey(service, name);
            List<BindingRegistration> registrations;
            lock (_bindingRegistrations)
            {
                _bindingRegistrations.TryGetValue(key, out registrations);
            }

            if (registrations != null && registrations.Count > 0)
            {
                var result = new object[registrations.Count];
                for (var i = 0; i < registrations.Count; i++)
                    result[i] = registrations[i].Resolve(parameters);
                return result;
            }

            if (_parent != null && _parent.HasRegistration(ref key))
                return _parent.GetAll(service, name, parameters);

            if (TryResolve(service, parameters, out var value))
                return new[] {value};
            return Default.EmptyArray<object>();
        }

        private object Resolve(Type service, IReadOnlyCollection<IIoCParameter>? parameters)
        {
            if (TryResolve(service, parameters, out var value))
                return value;
            if (service == typeof(IServiceProvider))
                return this;

            ExceptionManager.ThrowIoCCannotFindBinding(service);
            return null;
        }

        private bool TryResolve(Type service, IReadOnlyCollection<IIoCParameter>? parameters, out object? value)
        {
            if (service.IsArray)
            {
                var elementType = service.GetElementType();
                value = ConvertToArray(service, elementType);
                return true;
            }

            if (!service.IsGenericTypeUnified())
                return TryResolveSelfBindable(service, parameters, out value);


            var definition = service.GetGenericTypeDefinition();
            ConstructorInfo? constructor = null;
            var originalType = service.GetGenericArgumentsUnified()[0];
            if (definition.IsInterfaceUnified())
            {
                if (definition == typeof(ICollection<>) || definition == typeof(IEnumerable<>) || definition == typeof(IList<>)
                    || definition == typeof(IReadOnlyCollection<>) || definition == typeof(IReadOnlyList<>))
                    constructor = typeof(List<>).MakeGenericType(originalType).GetConstructorUnified(MemberFlags.InstancePublic, Default.EmptyArray<Type>());
            }
            else
            {
                if (typeof(ICollection<>).MakeGenericType(originalType).IsAssignableFromUnified(service))
                    constructor = service.GetConstructorUnified(MemberFlags.InstancePublic, Default.EmptyArray<Type>());
            }

            if (constructor == null)
                return TryResolveSelfBindable(service, parameters, out value);

            var methodInfo = constructor.DeclaringType?.GetMethodUnified("Add", MemberFlags.InstancePublic, originalType);
            if (methodInfo == null || methodInfo.IsStatic)
                return TryResolveSelfBindable(service, parameters, out value);

            var objects = GetAll(originalType, null, parameters);
            value = constructor.InvokeEx();
            var args = new object[1];
            for (var index = 0; index < objects.Length; index++)
            {
                args[0] = objects[index];
                methodInfo.InvokeEx(value, args);
            }

            return true;
        }

        private bool CanResolve(Type service, ref BindingKey key)
        {
            lock (_bindingRegistrations)
            {
                if (_bindingRegistrations.ContainsKey(key))
                    return true;
            }

            if (_parent == null)
                return IsSelfBindableType(service);
            return _parent.CanResolve(service, ref key);
        }

        private object ConvertToArray(Type arrayType, Type elementType)
        {
            var objects = GetAll(elementType, null, null);
            if (objects.Length == 1 && arrayType.IsInstanceOfTypeUnified(objects[0]))
                return Array.CreateInstance(elementType, 0);

            var array = Array.CreateInstance(elementType, objects.Length);
            for (var i = 0; i < objects.Length; i++)
                array.SetValue(objects[i], i);
            return array;
        }

        private bool TryResolveSelfBindable(Type service, IReadOnlyCollection<IIoCParameter>? parameters, out object? value)
        {
            BindingRegistration registration;
            lock (_selfActivatedRegistrations)
            {
                if (!_selfActivatedRegistrations.TryGetValue(service, out registration))
                {
                    if (IsSelfBindableType(service))
                        registration = new BindingRegistration(this, service, IoCDependencyLifecycle.Transient, null);
                    _selfActivatedRegistrations[service] = registration;
                }
            }

            if (registration == null)
            {
                value = null;
                return false;
            }

            value = registration.Resolve(parameters);
            return true;
        }

        private bool HasRegistration(ref BindingKey key)
        {
            bool result;
            lock (_bindingRegistrations)
            {
                result = _bindingRegistrations.ContainsKey(key);
            }

            if (result || _parent == null)
                return result;
            return _parent.HasRegistration(ref key);
        }

        private static bool IsSelfBindableType(Type service)
        {
            return !service.IsInterfaceUnified()
                   && !service.IsValueTypeUnified()
                   && service != typeof(string)
                   && !service.IsAbstractUnified()
                   && !service.ContainsGenericParametersUnified();
        }

        private void NotBeDisposed()
        {
            if (_isDisposed)
                ExceptionManager.ThrowObjectDisposed(GetType());
        }

        #endregion

        #region Nested types

        private sealed class BindingRegistration
        {
            #region Fields

            private readonly MugenIoCContainer _container;
            private readonly IoCDependencyLifecycle _lifecycle;
            private List<ConstructorInfo>? _cachedConstructors;
            private bool _hasValue;

            private bool _isActivating;
            private Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, object>? _methodBindingDelegate;
            private IReadOnlyCollection<IIoCParameter>? _parameters;
            private Type? _type;
            private object? _value;

            #endregion

            #region Constructors

            public BindingRegistration(MugenIoCContainer container, object value)
            {
                _container = container;
                _hasValue = true;
                _value = value;
                _lifecycle = IoCDependencyLifecycle.Singleton;
            }

            public BindingRegistration(MugenIoCContainer container, Func<IIoCContainer, IReadOnlyCollection<IIoCParameter>, object> methodBindingDelegate,
                IoCDependencyLifecycle lifecycle, IReadOnlyCollection<IIoCParameter>? parameters)
            {
                _container = container;
                _methodBindingDelegate = methodBindingDelegate;
                _lifecycle = lifecycle;
                _parameters = parameters;
            }

            public BindingRegistration(MugenIoCContainer container, Type type, IoCDependencyLifecycle lifecycle, IReadOnlyCollection<IIoCParameter>? parameters)
            {
                _container = container;
                _type = type;
                _lifecycle = lifecycle;
                _parameters = parameters;
            }

            #endregion

            #region Methods

            public object Resolve(IReadOnlyCollection<IIoCParameter>? parameters)
            {
                if (_hasValue)
                    return _value;

                var lockTaken = false;
                try
                {
                    Monitor.Enter(this, ref lockTaken);
                    if (_hasValue)
                        return _value;

                    if (_isActivating)
                        ExceptionManager.ThrowIoCCyclicalDependency(_type);

                    _isActivating = true;

                    parameters = MergeParameters(parameters);
                    if (_methodBindingDelegate != null)
                    {
                        if (_lifecycle == IoCDependencyLifecycle.Singleton)
                        {
                            _value = _methodBindingDelegate(_container, parameters);
                            _hasValue = true;
                            _methodBindingDelegate = null;
                            _parameters = null;
                            return _value;
                        }

                        return _methodBindingDelegate.Invoke(_container, parameters);
                    }

                    if (_type != null)
                    {
                        var constructor = FindConstructor(_type, parameters);
                        if (constructor == null)
                            ExceptionManager.ThrowCannotFindConstructor(_type);

                        var result = constructor.InvokeEx(GetParameters(constructor, parameters));
                        SetProperties(result, parameters);
                        (result as IInitializable)?.Initialize();
                        if (_lifecycle == IoCDependencyLifecycle.Singleton)
                        {
                            _value = result;
                            _hasValue = true;
                            _type = null;
                            _parameters = null;
                            return _value;
                        }

                        return result;
                    }

                    return _value;
                }
                finally
                {
                    _isActivating = false;
                    if (lockTaken)
                        Monitor.Exit(this);
                }
            }

            private void SetProperties(object item, IReadOnlyCollection<IIoCParameter> parameters)
            {
                if (parameters.Count == 0)
                    return;
                var type = item.GetType();
                foreach (var iocParameter in parameters)
                {
                    if (iocParameter.ParameterType != IoCParameterType.Property)
                        continue;
                    var propertyInfo = type.GetPropertyUnified(iocParameter.Name, _container.PropertyMemberFlags);
                    var methodInfo = propertyInfo.GetSetMethodUnified(false);
                    if (methodInfo != null && !methodInfo.IsStatic && methodInfo.IsPublic)
                        propertyInfo.SetValueEx(item, iocParameter.Value);
                }
            }

            private IReadOnlyCollection<IIoCParameter> MergeParameters(IReadOnlyCollection<IIoCParameter>? parameters)
            {
                if (_parameters == null || _parameters.Count == 0)
                    return parameters ?? Default.EmptyArray<IIoCParameter>();
                if (parameters == null || parameters.Count == 0)
                    return _parameters ?? Default.EmptyArray<IIoCParameter>();
                var iocParameters = new List<IIoCParameter>(parameters);
                iocParameters.AddRange(_parameters);
                return iocParameters;
            }

            private object?[] GetParameters(ConstructorInfo constructor, IReadOnlyCollection<IIoCParameter> parameters)
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
                        if (!CanResolve(injectionParameter, parameterInfo))
                            continue;
                        result[i] = injectionParameter.Value;
                        find = true;
                        break;
                    }

                    if (find)
                        continue;
                    var resolve = ResolveParameter(parameterInfo.ParameterType, parameterInfo);
                    result[i] = resolve;
                }

                return result;
            }

            private object? ResolveParameter(Type type, ParameterInfo parameterInfo)
            {
                //Use default parameter value.
                if (parameterInfo.IsOptional && !_container.CanResolve(type))
                    return parameterInfo.DefaultValue;

                var hasParamsAttr = parameterInfo.IsDefined(typeof(ParamArrayAttribute), false);
                //If have ParamArrayAttribute.
                if (hasParamsAttr)
                {
                    var originalType = type.GetElementType();

                    //If exist array binding.
                    if (_container.CanResolve(type))
                        return _container.Get(type);

                    //If exist binding for type.
                    if (_container.CanResolve(originalType))
                        return ConvertToArray(type, _container.GetAll(originalType, null, null));
                    return ConvertToArray(type, new object[0]);
                }

                return _container.Get(type);
            }

            private static object? ConvertToArray(Type arrayType, object obj)
            {
                if (obj == null)
                    return null;
                var objects = obj as Array;
                if (objects == null)
                    objects = new[] {obj};
                var array = (Array) Activator.CreateInstance(arrayType, objects.Length);
                for (var i = 0; i < objects.Length; i++)
                    array.SetValue(objects.GetValue(i), i);
                return array;
            }

            private ConstructorInfo? FindConstructor(Type service, IReadOnlyCollection<IIoCParameter> parameters)
            {
                ConstructorInfo? result = null;
                var bestCount = -1;
                var currentCountParameter = 0;
                var constructorInfos = GetConstructors(service);
                if (constructorInfos.Count == 1)
                    return constructorInfos[0];
                foreach (var constructorInfo in constructorInfos)
                {
                    var currentCount = 0;
                    var parameterInfos = constructorInfo.GetParameters();
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        foreach (var parameter in parameters)
                        {
                            if (CanResolve(parameter, parameterInfo))
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

            private static bool CanResolve(IIoCParameter parameter, ParameterInfo parameterInfo)
            {
                return parameter.ParameterType == IoCParameterType.Constructor && parameter.Name == parameterInfo.Name;
            }

            private List<ConstructorInfo> GetConstructors(Type service)
            {
                if (_cachedConstructors == null)
                {
                    _cachedConstructors = new List<ConstructorInfo>(service.GetConstructorsUnified(_container.ConstructorMemberFlags));
                    for (var index = 0; index < _cachedConstructors.Count; index++)
                    {
                        var constructorInfo = _cachedConstructors[index];
                        if (constructorInfo.IsStatic || !constructorInfo.IsPublic)
                        {
                            _cachedConstructors.Remove(constructorInfo);
                            index--;
                        }
                    }
                }

                return _cachedConstructors;
            }

            #endregion
        }

        private struct BindingKey
        {
            #region Fields

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private string? _name;
            private int _hashCode;

            public Type Type;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            public static readonly IEqualityComparer<BindingKey> TypeNameComparer = new TypeNameEqualityComparer();

            #endregion

            #region Constructors

            public BindingKey(Type type, string? name)
            {
                Type = type;
                _name = name;
                unchecked
                {
                    _hashCode = Type.GetHashCode() * 397 ^ (_name != null ? _name.GetHashCode() : 0);
                }
            }

            #endregion

            #region Nested types

            private sealed class TypeNameEqualityComparer : IEqualityComparer<BindingKey>
            {
                #region Implementation of interfaces

                public bool Equals(BindingKey x, BindingKey y)
                {
                    return x.Type.EqualsEx(y.Type) && string.Equals(x._name, y._name);
                }

                public int GetHashCode(BindingKey obj)
                {
                    return obj._hashCode;
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}